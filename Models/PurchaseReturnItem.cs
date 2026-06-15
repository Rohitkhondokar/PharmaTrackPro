using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A single returned quantity from a specific Batch. UnitCost is copied
    /// from the Batch at the moment of return (not looked up later), so the
    /// refund amount stays accurate even if the batch's cost were ever changed.
    /// </summary>
    public class PurchaseReturnItem
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseReturnId { get; set; }

        [ForeignKey(nameof(PurchaseReturnId))]
        public PurchaseReturn? PurchaseReturn { get; set; }

        [Required]
        public int BatchId { get; set; }

        [ForeignKey(nameof(BatchId))]
        public Batch? Batch { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Return quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantity * UnitCost;
    }
}
