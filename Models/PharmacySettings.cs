using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// System-wide business settings, stored as a single row. Genuinely
    /// user-editable preferences (thresholds, pharmacy contact info) live
    /// here rather than in appsettings.json, which stays reserved for
    /// environment/infrastructure config that shouldn't change at runtime.
    /// </summary>
    public class PharmacySettings
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        [Display(Name = "Pharmacy Name")]
        public string PharmacyName { get; set; } = "PharmaTrack Pro";

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [Range(0, 10000)]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 20;

        [Required]
        [Range(1, 720)]
        [Display(Name = "Near Expiry Threshold (days)")]
        public int NearExpiryDaysThreshold { get; set; } = 90;

        [Required]
        [StringLength(10)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "$";

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public string? UpdatedByUserId { get; set; }

        [ForeignKey(nameof(UpdatedByUserId))]
        public ApplicationUser? UpdatedByUser { get; set; }
    }
}
