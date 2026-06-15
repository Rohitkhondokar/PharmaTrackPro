using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface ISaleRepository : IGenericRepository<Sale>
    {
        Task<IEnumerable<Sale>> GetAllWithDetailsAsync();
        Task<Sale?> GetByIdWithDetailsAsync(int id);
    }
}
