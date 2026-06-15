using System.ComponentModel.DataAnnotations;

namespace PharmaTrackPro.ViewModels.PurchaseReturn
{
    public class PurchaseReturnItemInputViewModel
    {
        [Required]
        public int BatchId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Return quantity must be at least 1.")]
        public int Quantity { get; set; }
    }
}
