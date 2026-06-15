using PharmaTrackPro.ViewModels.Dashboard;

namespace PharmaTrackPro.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardAsync();
    }
}
