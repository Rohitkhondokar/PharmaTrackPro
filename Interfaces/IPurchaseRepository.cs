using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface IPurchaseRepository : IGenericRepository<Purchase>
    {
        Task<IEnumerable<Purchase>> GetAllWithDetailsAsync();
        Task<Purchase?> GetByIdWithDetailsAsync(int id);
    }
}
