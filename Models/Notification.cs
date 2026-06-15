using System.ComponentModel.DataAnnotations;
using PharmaTrackPro.Models.Enums;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A system-generated alert (low stock, near expiry, expired, pending
    /// purchase). These are broadcast to all staff, not scoped to a single
    /// user — everyone working the pharmacy needs to see the same operational
    /// alerts. Rows are created/removed lazily by NotificationService.SyncAsync()
    /// rather than by a background job; there's no scheduler in this stack.
    /// </summary>
    public class Notification
    {
        public int Id { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        /// <summary>Stable identifier for the underlying condition, e.g. "LowStock-42", used to avoid duplicate rows.</summary>
        [Required]
        [StringLength(100)]
        public string ReferenceKey { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Message { get; set; }

        /// <summary>Relative URL to navigate to when the notification is clicked.</summary>
        [StringLength(200)]
        public string? Link { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
    }
}
