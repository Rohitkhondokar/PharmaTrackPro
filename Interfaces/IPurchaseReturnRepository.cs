using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface IPurchaseReturnRepository : IGenericRepository<PurchaseReturn>
    {
        Task<IEnumerable<PurchaseReturn>> GetAllWithDetailsAsync();
        Task<PurchaseReturn?> GetByIdWithDetailsAsync(int id);
    }
}
