using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.Purchase
{
    public class PurchaseItemInputViewModel
    {
        [Required(ErrorMessage = "Select a medicine.")]
        public int MedicineId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }

        [Required]
        [Range(0, 999999.99, ErrorMessage = "Unit cost must be zero or greater.")]
        public decimal UnitCost { get; set; }

        [Required(ErrorMessage = "Batch number is required.")]
        [StringLength(50)]
        public string BatchNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Expiry date is required.")]
        public DateTime ExpiryDate { get; set; }
    }
}
