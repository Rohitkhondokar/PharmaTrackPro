using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.Purchase
{
    public class PurchaseFormViewModel
    {
        [Required(ErrorMessage = "Select a supplier.")]
        [Display(Name = "Supplier")]
        public int SupplierId { get; set; }

        [StringLength(50)]
        [Display(Name = "Invoice Number")]
        public string? InvoiceNumber { get; set; }

        [Required]
        [Display(Name = "Purchase Date")]
        public DateTime PurchaseDate { get; set; } = DateTime.Today;

        [StringLength(500)]
        public string? Notes { get; set; }

        [MinLength(1, ErrorMessage = "Add at least one line item.")]
        public List<PurchaseItemInputViewModel> Items { get; set; } = new();
    }
}
