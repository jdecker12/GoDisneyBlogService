using GoDisneyBlog.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDisneyBlog.Data
{
    public interface IImageRepository
    {
        string UploadImage(IFormFile imageFile);
    }
}
