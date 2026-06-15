using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaTrackPro.Models.Enums;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// Core catalog entity. Batch-level stock, expiry dates, and barcode/QR
    /// data are layered on top of this in later modules (Batch Management,
    /// Barcode & QR Code Management) rather than crammed in here — a Medicine
    /// is a catalog definition, not a physical stock unit.
    /// </summary>
    public class Medicine
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Generic Name")]
        public string? GenericName { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        [Required]
        public int ManufacturerId { get; set; }

        [ForeignKey(nameof(ManufacturerId))]
        public Manufacturer? Manufacturer { get; set; }

        [StringLength(30)]
        public string? Strength { get; set; }

        [Required]
        public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Tablet;

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        [Display(Name = "Purchase Price")]
        public decimal PurchasePrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999.99)]
        [Display(Name = "Selling Price")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Requires Prescription")]
        public bool RequiresPrescription { get; set; }

        // Product photo, relative web path e.g. "/uploads/medicines/{guid}.jpg"
        [StringLength(300)]
        public string? ImagePath { get; set; }

        // Auto-generated once, immediately after first save. Never changes afterward.
        [StringLength(50)]
        public string? Barcode { get; set; }

        [StringLength(300)]
        public string? BarcodeImagePath { get; set; }

        [StringLength(300)]
        public string? QrCodeImagePath { get; set; }

        public bool IsActive { get; set; } = true;

        // Soft delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<Batch> Batches { get; set; } = new List<Batch>();
    }
}
