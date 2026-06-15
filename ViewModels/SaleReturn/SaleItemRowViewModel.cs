namespace PharmaTrackPro.ViewModels.SaleReturn
{
    public class SaleItemRowViewModel
    {
        public int SaleItemId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public int OriginalQuantity { get; set; }
        public int AlreadyReturned { get; set; }
        public decimal UnitPrice { get; set; }

        public int Returnable => OriginalQuantity - AlreadyReturned;
    }
}
