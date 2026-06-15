using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaTrackPro.Models.Enums;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A purchase order placed with a supplier. Deliberately has no soft
    /// delete — it's a financial/audit record. Cancellation is tracked via
    /// Status, and once Received, the only way to reverse it is a future
    /// Purchase Return (Part 2), not editing or deleting this record.
    /// </summary>
    public class Purchase
    {
        public int Id { get; set; }

        [Required]
        public int SupplierId { get; set; }

        [ForeignKey(nameof(SupplierId))]
        public Supplier? Supplier { get; set; }

        [StringLength(50)]
        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;

        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CreatedByUserId))]
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReceivedAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        public ICollection<PurchaseItem> Items { get; set; } = new List<PurchaseItem>();
        public ICollection<PurchaseReturn> Returns { get; set; } = new List<PurchaseReturn>();
    }
}
