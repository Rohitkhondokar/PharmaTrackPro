using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A return of previously-received stock back to the supplier (damaged,
    /// wrong item, etc.). No soft delete — like Purchase, this is an audit
    /// record. Creating one immediately reduces the affected Batches'
    /// QuantityRemaining; there's no separate approval workflow.
    /// </summary>
    public class PurchaseReturn
    {
        public int Id { get; set; }

        [Required]
        public int PurchaseId { get; set; }

        [ForeignKey(nameof(PurchaseId))]
        public Purchase? Purchase { get; set; }

        [Required]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Refund Amount")]
        public decimal TotalRefundAmount { get; set; }

        [Required]
        public string CreatedByUserId { get; set; } = string.Empty;

        [ForeignKey(nameof(CreatedByUserId))]
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PurchaseReturnItem> Items { get; set; } = new List<PurchaseReturnItem>();
    }
}
