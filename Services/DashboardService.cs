using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Enums;
using PharmaTrackPro.ViewModels.Dashboard;

namespace PharmaTrackPro.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IInventoryService _inventoryService;
        private readonly IExpiryService _expiryService;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly ISettingsService _settingsService;

        public DashboardService(
            IInventoryService inventoryService,
            IExpiryService expiryService,
            IPurchaseRepository purchaseRepository,
            IBatchRepository batchRepository,
            ISettingsService settingsService)
        {
            _inventoryService = inventoryService;
            _expiryService = expiryService;
            _purchaseRepository = purchaseRepository;
            _batchRepository = batchRepository;
            _settingsService = settingsService;
        }

        public async Task<DashboardViewModel> GetDashboardAsync()
        {
            var inventorySummary = (await _inventoryService.GetInventorySummaryAsync()).ToList();
            var expiryReport = (await _expiryService.GetExpiryReportAsync()).ToList();
            var purchases = (await _purchaseRepository.GetAllWithDetailsAsync()).ToList();
            var batches = (await _batchRepository.GetAllWithMedicineAsync()).ToList();

            var settings = await _settingsService.GetSettingsAsync();
            var nearExpiryDays = settings.NearExpiryDaysThreshold;

            var dashboard = new DashboardViewModel
            {
                TotalMedicines = inventorySummary.Count,
                LowStockCount = inventorySummary.Count(i => i.IsLowStock),
                ExpiredCount = expiryReport.Count(b => b.IsExpired),
                NearExpiryCount = expiryReport.Count(b => !b.IsExpired && b.DaysUntilExpiry <= nearExpiryDays),
                TotalInventoryValue = batches.Sum(b => b.QuantityRemaining * b.UnitCost),
                PendingPurchasesCount = purchases.Count(p => p.Status == PurchaseStatus.Pending)
            };

            dashboard.RecentPurchases = purchases
                .OrderByDescending(p => p.PurchaseDate)
                .Take(5)
                .Select(p => new RecentPurchaseViewModel
                {
                    Id = p.Id,
                    SupplierName = p.Supplier?.Name ?? "—",
                    PurchaseDate = p.PurchaseDate,
                    Status = p.Status.ToString(),
                    TotalAmount = p.TotalAmount
                })
                .ToList();

            // Last 6 months of purchase spend, including months with zero activity
            var months = new List<MonthlySpendViewModel>();
            var today = DateTime.Today;
            for (int i = 5; i >= 0; i--)
            {
                var monthStart = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
                var monthEnd = monthStart.AddMonths(1);

                var total = purchases
                    .Where(p => p.Status != PurchaseStatus.Cancelled && p.PurchaseDate >= monthStart && p.PurchaseDate < monthEnd)
                    .Sum(p => p.TotalAmount);

                months.Add(new MonthlySpendViewModel
                {
                    MonthLabel = monthStart.ToString("MMM yyyy"),
                    Total = total
                });
            }
            dashboard.MonthlyPurchaseSpend = months;

            dashboard.StockByCategory = inventorySummary
                .GroupBy(i => i.CategoryName)
                .Select(g => new CategoryStockViewModel
                {
                    CategoryName = g.Key,
                    TotalQuantity = g.Sum(i => i.TotalQuantity)
                })
                .OrderByDescending(c => c.TotalQuantity)
                .Take(8)
                .ToList();

            return dashboard;
        }
    }
}
