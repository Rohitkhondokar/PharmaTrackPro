namespace PharmaTrackPro.ViewModels.Inventory
{
    public class InventoryItemViewModel
    {
        public int MedicineId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Strength { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ManufacturerName { get; set; } = string.Empty;
        public string UnitOfMeasure { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
        public decimal SellingPrice { get; set; }
        public bool IsLowStock { get; set; }
        public bool HasExpiredBatches { get; set; }
        public bool HasNearExpiryBatches { get; set; }
    }
}
