using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<Customer?> GetByIdIncludingDeletedAsync(int id);
        Task<IEnumerable<Customer>> GetAllIncludingDeletedAsync();
        Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
    }
}
