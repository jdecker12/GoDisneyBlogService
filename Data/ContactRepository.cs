using GoDisneyBlog.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoDisneyBlog.Data
{
    public class ContactRepository : IContactRepository
    {
        private GoDisneyContext _context;
        private ILogger<GoDisneyRepository> _logger;

        public ContactRepository(GoDisneyContext context, ILogger<GoDisneyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public void AddEntity(object model)
        {
            _context.Add(model);
        }


        public void DeleteEntity(object model)
        {
            _context.Remove(model);
        }

        public async Task<IEnumerable<ContactForm>> GetAllEmail()
        {
            return await _context.ContactForms
                        .ToListAsync();
        }

        public async Task<ContactForm?> GetByEmail(string email)
        {
           try
            {
                var result = await _context.ContactForms
                       .Where(x => x.Email == email)
                       .FirstOrDefaultAsync();

                if (result == null) return null;

                return result;
            }
            catch(Exception ex)
            {
                _logger.LogError($"Could not find message {ex}");
                return null;
            }
        }

        public async Task<bool> SaveAllAsync()
        {
            try
            {
                return (await _context.SaveChangesAsync()) > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save all cahnges {ex}");
                return false;
            }
        }
    }
}
