using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Repositories
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Customer?> GetByIdIncludingDeletedAsync(int id) =>
            await DbSet.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.Id == id);

        public async Task<IEnumerable<Customer>> GetAllIncludingDeletedAsync() =>
            await DbSet.IgnoreQueryFilters().OrderBy(c => c.Name).ToListAsync();

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        {
            var query = DbSet.Where(c => c.Phone != null && c.Phone == phone);

            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
