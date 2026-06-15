namespace PharmaTrackPro.ViewModels.SaleReturn
{
    public class SaleReturnCreateViewModel
    {
        public int SaleId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<SaleItemRowViewModel> ReturnableItems { get; set; } = new();
    }
}
