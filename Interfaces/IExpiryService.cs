using PharmaTrackPro.ViewModels.Expiry;

namespace PharmaTrackPro.Interfaces
{
    public interface IExpiryService
    {
        /// <summary>All batches with remaining stock, annotated with expiry status — expired first, then soonest-expiring.</summary>
        Task<IEnumerable<ExpiringBatchViewModel>> GetExpiryReportAsync();
    }
}
