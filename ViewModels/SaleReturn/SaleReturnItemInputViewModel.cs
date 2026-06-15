using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.SaleReturn
{
    public class SaleReturnItemInputViewModel
    {
        [Required]
        public int SaleItemId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Return quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
