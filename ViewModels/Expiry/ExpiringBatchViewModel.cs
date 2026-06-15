namespace PharmaTrackPro.ViewModels.Expiry
{
    public class ExpiringBatchViewModel
    {
        public int BatchId { get; set; }
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public string BatchNumber { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public int QuantityRemaining { get; set; }
        public decimal UnitCost { get; set; }
        public bool IsExpired { get; set; }
    }
}
