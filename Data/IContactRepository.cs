using GoDisneyBlog.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDisneyBlog.Data
{
    public interface IContactRepository
    {
        Task<IEnumerable<ContactForm>> GetAllEmail();
        Task<ContactForm> GetByEmail(string email);
        void AddEntity(object model);
        void DeleteEntity(object model);
        Task<bool> SaveAllAsync();
    }
}
