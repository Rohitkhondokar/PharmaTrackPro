using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<Supplier?> GetByIdIncludingDeletedAsync(int id);
        Task<IEnumerable<Supplier>> GetAllIncludingDeletedAsync();
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
