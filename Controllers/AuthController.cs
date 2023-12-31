﻿using GoDisneyBlog.Data.Entities;
using GoDisneyBlog.Data;
using GoDisneyBlog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GoDisneyBlog.Controllers
{
    [Route("api/[Controller]")]
    public class AuthController : Controller
    {

        private ILogger _logger;
        private SignInManager<StoreUser> _signInManager;
        private UserManager<StoreUser> _userManager;
        private IConfiguration _config;
        private IGoDisneyRepository _repository;

        public AuthController(ILogger<AuthController> logger, SignInManager<StoreUser> signInManager, UserManager<StoreUser> userManager, IConfiguration config, IGoDisneyRepository repository)
        {
            _logger = logger;
            _signInManager = signInManager;
            _userManager = userManager;
            _config = config;
            _repository = repository;

        }

        public IActionResult Login()
        {
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "FallBack");
            }

            return View();
        }

        [HttpPost]
        [Route("CreateToken")]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            string uName = model.Username;
            string uPass = model.Password;

            byte[] decName = Convert.FromBase64String(uName);
            byte[] decPass = Convert.FromBase64String(uPass);

            string decodeUser = Encoding.UTF8.GetString(decName);
            string decodedPassword = Encoding.UTF8.GetString(decPass);

            model.Username = decodeUser;
            model.Password = decodedPassword;

            if (ModelState.IsValid)
            {

                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                    if (result.Succeeded)
                    {
                        ///create token

                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
                        };

                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _config["Tokens:Issuer"],
                            _config["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(30),
                            signingCredentials: creds
                            );
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };
                        return Created("", results);
                    }
                }
            }

            return BadRequest();
        }

        [HttpGet("ValidateToken/{token}")]
        public IActionResult ValidateToken(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Tokens:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Tokens:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"])),
                ValidateLifetime = true,
            };

            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
                return Ok(new {message = tokenValidationParameters });
            }
            catch
            {
                return Unauthorized("Token is invalid");
            }
        }

        [HttpPost("RegisterUser")]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterViewModel model)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    user = new StoreUser { FName = model.FName, LName = model.FName, Email = model.Email, UserName = model.UserName };
                }
              
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    if (model.UserRole == String.Empty || model.UserRole == null || model.UserRole == "User")
                    {
                        await _userManager.AddToRolesAsync(user, new List<string> { "User" });
                    }
                    else
                    {
                        await _userManager.AddToRolesAsync(user, new List<string> { model.UserRole });
                    }
                   
                    return Ok(result.Succeeded);
                }
                else
                {
                    return BadRequest("Could not create user");
                }
            }
            catch(Exception ex)
            {
                return BadRequest($"Could not create user");
                _logger.LogError($"Could not create user {ex}");
            }
        }

        [HttpGet("GetUserRole/{email}")]
        public async Task<IActionResult> GetUserRole(string email)
        {
            try
            {
                StoreUser user = await _userManager.FindByNameAsync(email);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }

                var userRole =  await _userManager.GetRolesAsync(user);
                if (userRole == null)
                {
                    throw new ArgumentException("Role not found");
                }
                return Ok(userRole);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Could not find user {ex}");
                return BadRequest($"Could not find user {ex}");
            }

        }

        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(userId);
                if (user == null)
                {
                    throw new ArgumentException("User not found");
                }
                var roles = await _userManager.GetRolesAsync(user);
         
                    foreach(var role in roles)
                    {
                        await _userManager.RemoveFromRolesAsync(user, new List<string> { role });
                    }
                

                var result = await _userManager.DeleteAsync(user);
                    return Ok(result.Succeeded);
      
            }
            catch(Exception ex)
            {
                _logger.LogError("Could not delete user");
                return BadRequest($"Could not delete user {ex}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> StoreKey([FromBody] RememberMe model)
        {

            var newKey = model;


            try
            {

                _repository.AddEntity(newKey);
                if (await _repository.SaveAllAsync())
                {

                    return Created($"/Auth/StoreKey/{newKey.Id}", newKey);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save new key. {ex}");

            }
            return BadRequest(ModelState);

        }
    }
}
