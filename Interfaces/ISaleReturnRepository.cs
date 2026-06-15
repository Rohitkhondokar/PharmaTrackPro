using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface ISaleReturnRepository : IGenericRepository<SaleReturn>
    {
        Task<IEnumerable<SaleReturn>> GetAllWithDetailsAsync();
        Task<SaleReturn?> GetByIdWithDetailsAsync(int id);

        /// <summary>Total already-returned quantity per SaleItem, for a given sale.</summary>
        Task<Dictionary<int, int>> GetReturnedQuantitiesForSaleAsync(int saleId);
    }
}
