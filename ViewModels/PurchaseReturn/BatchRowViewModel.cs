namespace PharmaTrackPro.ViewModels.PurchaseReturn
{
    public class BatchRowViewModel
    {
        public int BatchId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int QuantityRemaining { get; set; }
        public decimal UnitCost { get; set; }
    }
}
