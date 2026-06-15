using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;

namespace PharmaTrackPro.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetRecent()
        {
            await _notificationService.SyncAsync();

            var notifications = await _notificationService.GetRecentAsync(10);
            var unreadCount = await _notificationService.GetUnreadCountAsync();

            var data = notifications.Select(n => new
            {
                n.Id,
                Type = n.Type.ToString(),
                n.Title,
                n.Message,
                n.Link,
                n.IsRead,
                CreatedAt = n.CreatedAt.ToString("MMM dd, HH:mm")
            });

            return Json(new { data, unreadCount });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await _notificationService.SyncAsync();

            var notifications = await _notificationService.GetRecentAsync(200);

            var data = notifications.Select(n => new
            {
                n.Id,
                Type = n.Type.ToString(),
                n.Title,
                n.Message,
                n.Link,
                n.IsRead,
                CreatedAt = n.CreatedAt.ToString("MMM dd, yyyy HH:mm")
            });

            return Json(new { data });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _notificationService.MarkAsReadAsync(id);
            return Json(new { result.Success, result.Message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var result = await _notificationService.MarkAllAsReadAsync();
            return Json(new { result.Success, result.Message });
        }
    }
}
