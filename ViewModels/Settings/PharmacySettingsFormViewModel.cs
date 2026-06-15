using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.Settings
{
    public class PharmacySettingsFormViewModel
    {
        [Required(ErrorMessage = "Pharmacy name is required.")]
        [StringLength(150, MinimumLength = 2)]
        [Display(Name = "Pharmacy Name")]
        public string PharmacyName { get; set; } = string.Empty;

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(20)]
        [Phone(ErrorMessage = "Enter a valid phone number.")]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string? Email { get; set; }

        [Required]
        [Range(0, 10000, ErrorMessage = "Enter a value between 0 and 10,000.")]
        [Display(Name = "Low Stock Threshold")]
        public int LowStockThreshold { get; set; } = 20;

        [Required]
        [Range(1, 720, ErrorMessage = "Enter a value between 1 and 720 days.")]
        [Display(Name = "Near Expiry Threshold (days)")]
        public int NearExpiryDaysThreshold { get; set; } = 90;

        [Required]
        [StringLength(10, MinimumLength = 1)]
        [Display(Name = "Currency Symbol")]
        public string CurrencySymbol { get; set; } = "$";
    }
}
