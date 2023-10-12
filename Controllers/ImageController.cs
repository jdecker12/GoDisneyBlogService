using GoDisneyBlog.Data.Entities;
using GoDisneyBlog.Data;
using GoDisneyBlog.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

namespace GoDisneyBlog.Controllers
{
    [Route("api/[Controller]")]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ImageController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;
        private ILogger<ImageController> _logger;

        public ImageController(IImageRepository imageRepository, ILogger<ImageController> logger)
        {
            _imageRepository = imageRepository;
            _logger = logger;
        }
        [HttpGet]
        [Route("GetTest")]
        [AllowAnonymous]
        public IActionResult GetTest()
        {
            var message = new { message = "Message works!" };
            return Ok(message);
        }

        [HttpPost]
        [Route("UploadImage")]
        [AllowAnonymous]
        public IActionResult UploadImage([FromForm] IFormFile uploadFileInput)
        {
            try
            {
                var imagePath = _imageRepository.UploadImage(uploadFileInput);
                return Ok(new { ImagePath = imagePath });
            }
            catch (ArgumentNullException ex)
            {
                _logger.LogError($"Failed to upload image {ex}");
            }
            return BadRequest("Failed to upload image");
        }
    
    }
}
