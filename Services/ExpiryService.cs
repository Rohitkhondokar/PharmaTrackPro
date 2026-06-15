using PharmaTrackPro.Interfaces;
using PharmaTrackPro.ViewModels.Expiry;

namespace PharmaTrackPro.Services
{
    public class ExpiryService : IExpiryService
    {
        private readonly IBatchRepository _batchRepository;

        public ExpiryService(IBatchRepository batchRepository)
        {
            _batchRepository = batchRepository;
        }

        public async Task<IEnumerable<ExpiringBatchViewModel>> GetExpiryReportAsync()
        {
            var today = DateTime.Today;
            var batches = await _batchRepository.GetAllWithMedicineAsync();

            var report = batches
                .Where(b => b.QuantityRemaining > 0 && b.Medicine != null)
                .Select(b => new ExpiringBatchViewModel
                {
                    BatchId = b.Id,
                    MedicineId = b.MedicineId,
                    MedicineName = b.Medicine!.Name,
                    Strength = b.Medicine.Strength,
                    BatchNumber = b.BatchNumber,
                    ExpiryDate = b.ExpiryDate,
                    DaysUntilExpiry = (b.ExpiryDate.Date - today).Days,
                    QuantityRemaining = b.QuantityRemaining,
                    UnitCost = b.UnitCost,
                    IsExpired = b.ExpiryDate.Date < today
                })
                .OrderBy(b => b.ExpiryDate) // soonest/most-overdue first
                .ToList();

            return report;
        }
    }
}
