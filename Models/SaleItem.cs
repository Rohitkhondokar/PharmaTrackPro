using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PharmaTrackPro.Models
{
    /// <summary>
    /// A single sold quantity drawn from a specific Batch. If a cart line's
    /// requested quantity spans more than one batch (FEFO exhausts the
    /// oldest-expiring batch first), the sale produces multiple SaleItem rows
    /// for that medicine — one per batch drawn from. This keeps stock
    /// traceability accurate at the batch level.
    /// </summary>
    public class SaleItem
    {
        public int Id { get; set; }

        [Required]
        public int SaleId { get; set; }

        [ForeignKey(nameof(SaleId))]
        public Sale? Sale { get; set; }

        [Required]
        public int MedicineId { get; set; }

        [ForeignKey(nameof(MedicineId))]
        public Medicine? Medicine { get; set; }

        [Required]
        public int BatchId { get; set; }

        [ForeignKey(nameof(BatchId))]
        public Batch? Batch { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
