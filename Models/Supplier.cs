using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A vendor/distributor the pharmacy purchases stock from.
    /// Distinct from Manufacturer: a Manufacturer makes the drug,
    /// a Supplier is who you actually order it from and pay.
    /// Referenced by Purchase once the Purchase Management module is built.
    /// </summary>
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Contact Person")]
        public string? ContactPerson { get; set; }

        [StringLength(20)]
        [Phone]
        public string? Phone { get; set; }

        [StringLength(150)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        [StringLength(50)]
        [Display(Name = "License / Registration Number")]
        public string? LicenseNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; }

        public bool IsActive { get; set; } = true;

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}
