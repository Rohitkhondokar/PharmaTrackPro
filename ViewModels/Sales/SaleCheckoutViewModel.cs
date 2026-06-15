using System.ComponentModel.DataAnnotations;
using PharmaTrackPro.Models.Enums;

namespace PharmaTrackPro.ViewModels.Sales
{
    public class SaleCartItemViewModel
    {
        [Required]
        public int MedicineId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; }
    }

    public class SaleCheckoutViewModel
    {
        // Nullable — walk-in sale with no customer on file.
        public int? CustomerId { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        [Display(Name = "Prescription Verified")]
        public bool PrescriptionVerified { get; set; }

        [MinLength(1, ErrorMessage = "Cart is empty.")]
        public List<SaleCartItemViewModel> Items { get; set; } = new();
    }
}
