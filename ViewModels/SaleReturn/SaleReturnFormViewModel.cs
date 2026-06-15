using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.SaleReturn
{
    public class SaleReturnFormViewModel
    {
        [Required]
        public int SaleId { get; set; }

        [Required]
        [Display(Name = "Return Date")]
        public DateTime ReturnDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "A reason is required for the return.")]
        [StringLength(500)]
        public string Reason { get; set; } = string.Empty;

        [MinLength(1, ErrorMessage = "Enter a return quantity for at least one item.")]
        public List<SaleReturnItemInputViewModel> Items { get; set; } = new();
    }
}
