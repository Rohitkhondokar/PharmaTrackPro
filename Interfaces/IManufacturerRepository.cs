using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface IManufacturerRepository : IGenericRepository<Manufacturer>
    {
        Task<Manufacturer?> GetByIdIncludingDeletedAsync(int id);
        Task<IEnumerable<Manufacturer>> GetAllIncludingDeletedAsync();
        Task<bool> NameExistsAsync(string name, int? excludeId = null);
    }
}
