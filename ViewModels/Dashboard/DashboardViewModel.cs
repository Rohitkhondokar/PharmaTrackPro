namespace PharmaTrackPro.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int TotalMedicines { get; set; }
        public int LowStockCount { get; set; }
        public int NearExpiryCount { get; set; }
        public int ExpiredCount { get; set; }
        public decimal TotalInventoryValue { get; set; }
        public int PendingPurchasesCount { get; set; }

        public List<RecentPurchaseViewModel> RecentPurchases { get; set; } = new();
        public List<MonthlySpendViewModel> MonthlyPurchaseSpend { get; set; } = new();
        public List<CategoryStockViewModel> StockByCategory { get; set; } = new();
    }

    public class RecentPurchaseViewModel
    {
        public int Id { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime PurchaseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class MonthlySpendViewModel
    {
        public string MonthLabel { get; set; } = string.Empty;
        public decimal Total { get; set; }
    }

    public class CategoryStockViewModel
    {
        public string CategoryName { get; set; } = string.Empty;
        public int TotalQuantity { get; set; }
    }
}
