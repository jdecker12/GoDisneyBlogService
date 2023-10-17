using AutoMapper;
using GoDisneyBlog.Data.Entities;
using GoDisneyBlog.Data;
using GoDisneyBlog.ViewModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Text;

namespace GoDisneyBlog.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    //[ApiController]
    public class ContactController : ControllerBase
    {
        private IContactRepository _repository;
        private ILogger<ContactController> _logger;
        private IMapper _mapper;
        private UserManager<StoreUser> _userManager;
        private SignInManager<StoreUser> _signInManager;

        public ContactController (IContactRepository repository, 
                ILogger<ContactController> logger,
                IMapper mapper, 
                UserManager<StoreUser> userManager,
                SignInManager<StoreUser> signInManager)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
        }


        [HttpGet]
        [Route("GetAllEmail")]
        public async Task<IActionResult> GetAllEmail()
        {
            try
            {
                var emails = await _repository.GetAllEmail();
                return Ok(_mapper.Map<IEnumerable<ContactForm>, IEnumerable<ContactFormViewModel>>(emails));
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to get emails {ex}");
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete]
        [Route("DeleteEmail/{email}")]
        public async Task<IActionResult> DeleteEmail(string email)
        {
            try
            {
                var oldEmail = await _repository.GetByEmail(email);
                if (oldEmail == null) return NotFound($"Could not find this  email {email}");
   
                _repository.DeleteEntity(oldEmail);

                if (await _repository.SaveAllAsync())
                {
                    return Ok(new { Email = oldEmail.Email });
                }
               
            }
            catch(Exception ex)
            {
                _logger.LogError($"Could not delete email {ex}");
            }

            return BadRequest();
        }

        [HttpPost]
        [Route("SaveContact")]
        [AllowAnonymous]
        public async Task<IActionResult> SaveContact([FromBody] ContactForm model)
        {
            try
            {
                _repository.AddEntity(model);
                if (await _repository.SaveAllAsync())
                {
                    return Created($"/api/contact/{model.Id}", _mapper.Map<ContactForm, ContactFormViewModel>(model));
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"Failed to save Contact info {ex}");
            }

            return BadRequest();

        }
    }
}
