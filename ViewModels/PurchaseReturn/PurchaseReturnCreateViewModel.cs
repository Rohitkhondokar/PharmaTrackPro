namespace PharmaTrackPro.ViewModels.PurchaseReturn
{
    public class PurchaseReturnCreateViewModel
    {
        public int PurchaseId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public List<BatchRowViewModel> AvailableBatches { get; set; } = new();
    }
}
