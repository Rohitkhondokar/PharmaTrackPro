using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface IMedicineRepository : IGenericRepository<Medicine>
    {
        /// <summary>Returns medicines with Category/Manufacturer eagerly loaded for list display.</summary>
        Task<IEnumerable<Medicine>> GetAllWithDetailsAsync(bool includeDeleted = false);

        Task<Medicine?> GetByIdWithDetailsAsync(int id);

        Task<Medicine?> GetByIdIncludingDeletedAsync(int id);

        Task<bool> NameStrengthExistsAsync(string name, string? strength, int? excludeId = null);

        Task<bool> BarcodeExistsAsync(string barcode);
    }
}
