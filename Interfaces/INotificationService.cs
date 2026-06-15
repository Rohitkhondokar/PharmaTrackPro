using PharmaTrackPro.Helpers;
using PharmaTrackPro.Models;

namespace PharmaTrackPro.Interfaces
{
    public interface INotificationService
    {
        /// <summary>Regenerates notifications from current system conditions — creates new ones, removes resolved ones.</summary>
        Task SyncAsync();

        Task<IEnumerable<Notification>> GetRecentAsync(int take = 20);
        Task<int> GetUnreadCountAsync();
        Task<ServiceResult> MarkAsReadAsync(int id);
        Task<ServiceResult> MarkAllAsReadAsync();
    }
}
