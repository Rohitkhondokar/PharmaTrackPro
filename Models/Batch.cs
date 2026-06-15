using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A physical unit of stock: a specific batch/lot of a medicine received
    /// on a specific date, with its own expiry and remaining quantity.
    /// Created automatically when a Purchase is marked Received. The future
    /// Sales module deducts from QuantityRemaining; Inventory/Expiry modules
    /// read from this table directly.
    /// </summary>
    public class Batch
    {
        public int Id { get; set; }

        [Required]
        public int MedicineId { get; set; }

        [ForeignKey(nameof(MedicineId))]
        public Medicine? Medicine { get; set; }

        // Nullable: in principle a batch could be created via a manual
        // stock adjustment later, not only through a purchase.
        public int? PurchaseItemId { get; set; }

        [ForeignKey(nameof(PurchaseItemId))]
        public PurchaseItem? PurchaseItem { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Batch Number")]
        public string BatchNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Expiry Date")]
        public DateTime ExpiryDate { get; set; }

        [Required]
        [Display(Name = "Quantity Received")]
        public int QuantityReceived { get; set; }

        [Required]
        [Display(Name = "Quantity Remaining")]
        public int QuantityRemaining { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Cost")]
        public decimal UnitCost { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
