using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A returned quantity against a specific original SaleItem. UnitPrice is
    /// copied from the SaleItem at the moment of return, so the refund stays
    /// accurate even if the medicine's price changes later.
    /// </summary>
    public class SaleReturnItem
    {
        public int Id { get; set; }

        [Required]
        public int SaleReturnId { get; set; }

        [ForeignKey(nameof(SaleReturnId))]
        public SaleReturn? SaleReturn { get; set; }

        [Required]
        public int SaleItemId { get; set; }

        [ForeignKey(nameof(SaleItemId))]
        public SaleItem? SaleItem { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Return quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
