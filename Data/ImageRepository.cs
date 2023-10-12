using GoDisneyBlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDisneyBlog.Data
{
    public class ImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ImageRepository(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public string UploadImage(IFormFile imageFile)
        {
           var uniqueFileName = Guid.NewGuid().ToString() + "_" + imageFile.FileName;
           var uploadDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "assets", "img");
           var filePath = Path.Combine(uploadDirectory, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                imageFile.CopyTo(fileStream);
            }

            return uniqueFileName;
        }
    }
}
