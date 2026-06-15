using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PharmaTrackPro.Models.Identity;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A return of previously-sold medicine back into stock. No soft delete —
    /// like SaleReturn's Purchase counterpart, this is an audit record.
    /// Creating one immediately restocks the originating Batch.
    /// </summary>
    public class SaleReturn
    {
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        [ForeignKey(nameof(SaleId))]
        public Sale? Sale { get; set; }

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

        public ICollection<SaleReturnItem> Items { get; set; } = new List<SaleReturnItem>();
    }
}
