using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.Models.Enums;

namespace PharmaTrackPro.Services
{
    public class NotificationService : INotificationService
    {
        private static readonly string[] ManagedPrefixes = { "LowStock-", "NearExpiry-", "Expired-", "PendingPurchase-" };

        private readonly IGenericRepository<Notification> _notificationRepository;
        private readonly IInventoryService _inventoryService;
        private readonly IExpiryService _expiryService;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISettingsService _settingsService;

        public NotificationService(
            IGenericRepository<Notification> notificationRepository,
            IInventoryService inventoryService,
            IExpiryService expiryService,
            IPurchaseRepository purchaseRepository,
            ISettingsService settingsService)
        {
            _notificationRepository = notificationRepository;
            _inventoryService = inventoryService;
            _expiryService = expiryService;
            _purchaseRepository = purchaseRepository;
            _settingsService = settingsService;
        }

        public async Task SyncAsync()
        {
            var active = new Dictionary<string, (NotificationType Type, string Title, string? Message, string? Link)>();
            var settings = await _settingsService.GetSettingsAsync();

            var inventory = await _inventoryService.GetInventorySummaryAsync();
            foreach (var item in inventory.Where(i => i.IsLowStock))
            {
                active[$"LowStock-{item.MedicineId}"] = (
                    NotificationType.LowStock,
                    $"Low stock: {item.Name}",
                    $"Only {item.TotalQuantity} unit(s) remaining.",
                    "/Inventory");
            }

            var expiryReport = await _expiryService.GetExpiryReportAsync();
            foreach (var batch in expiryReport.Where(b => b.IsExpired))
            {
                active[$"Expired-{batch.BatchId}"] = (
                    NotificationType.Expired,
                    $"Expired stock: {batch.MedicineName}",
                    $"Batch {batch.BatchNumber} expired {Math.Abs(batch.DaysUntilExpiry)} day(s) ago.",
                    "/Expiry");
            }
            foreach (var batch in expiryReport.Where(b => !b.IsExpired))
            {
                if (batch.DaysUntilExpiry <= settings.NearExpiryDaysThreshold)
                {
                    active[$"NearExpiry-{batch.BatchId}"] = (
                        NotificationType.NearExpiry,
                        $"Expiring soon: {batch.MedicineName}",
                        $"Batch {batch.BatchNumber} expires in {batch.DaysUntilExpiry} day(s).",
                        "/Expiry");
                }
            }

            var purchases = await _purchaseRepository.GetAllWithDetailsAsync();
            foreach (var purchase in purchases.Where(p => p.Status == Models.Enums.PurchaseStatus.Pending))
            {
                active[$"PendingPurchase-{purchase.Id}"] = (
                    NotificationType.PendingPurchase,
                    $"Purchase order #{purchase.Id} pending",
                    $"Order from {purchase.Supplier?.Name ?? "supplier"} awaiting receipt.",
                    $"/Purchases/Details/{purchase.Id}");
            }

            var existing = (await _notificationRepository.GetAllAsync()).ToList();
            var existingKeys = existing.Select(n => n.ReferenceKey).ToHashSet();

            foreach (var (key, value) in active)
            {
                if (!existingKeys.Contains(key))
                {
                    await _notificationRepository.AddAsync(new Notification
                    {
                        Type = value.Type,
                        ReferenceKey = key,
                        Title = value.Title,
                        Message = value.Message,
                        Link = value.Link,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            foreach (var notification in existing)
            {
                var isManaged = ManagedPrefixes.Any(prefix => notification.ReferenceKey.StartsWith(prefix));
                if (isManaged && !active.ContainsKey(notification.ReferenceKey))
                {
                    _notificationRepository.Remove(notification);
                }
            }

            await _notificationRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetRecentAsync(int take = 20)
        {
            var all = await _notificationRepository.GetAllAsync();
            return all
                .OrderBy(n => n.IsRead)
                .ThenByDescending(n => n.CreatedAt)
                .Take(take)
                .ToList();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            var all = await _notificationRepository.GetAllAsync();
            return all.Count(n => !n.IsRead);
        }

        public async Task<ServiceResult> MarkAsReadAsync(int id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification is null)
            {
                return ServiceResult.Fail("Notification not found.");
            }

            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;

            _notificationRepository.Update(notification);
            await _notificationRepository.SaveChangesAsync();

            return ServiceResult.Ok("Marked as read.");
        }

        public async Task<ServiceResult> MarkAllAsReadAsync()
        {
            var all = await _notificationRepository.GetAllAsync();
            var unread = all.Where(n => !n.IsRead).ToList();

            foreach (var notification in unread)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _notificationRepository.Update(notification);
            }

            await _notificationRepository.SaveChangesAsync();

            return ServiceResult.Ok("All notifications marked as read.");
        }
    }
}
