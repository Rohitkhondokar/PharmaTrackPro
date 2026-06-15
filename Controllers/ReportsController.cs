using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaTrackPro.Interfaces;

namespace PharmaTrackPro.Controllers
{
    [Authorize(Roles = "Administrator,StoreManager")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public IActionResult Index()
        {
            ViewBag.DefaultFrom = DateTime.Today.AddDays(-30).ToString("yyyy-MM-dd");
            ViewBag.DefaultTo = DateTime.Today.ToString("yyyy-MM-dd");
            return View();
        }

        // ---------------- Sales ----------------

        public async Task<IActionResult> SalesReportPdf(DateTime from, DateTime to)
        {
            var bytes = await _reportService.GenerateSalesReportPdfAsync(from, to);
            return File(bytes, "application/pdf", $"SalesReport_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> SalesReportExcel(DateTime from, DateTime to)
        {
            var bytes = await _reportService.GenerateSalesReportExcelAsync(from, to);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SalesReport_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }

        // ---------------- Purchases ----------------

        public async Task<IActionResult> PurchaseReportPdf(DateTime from, DateTime to)
        {
            var bytes = await _reportService.GeneratePurchaseReportPdfAsync(from, to);
            return File(bytes, "application/pdf", $"PurchaseReport_{from:yyyyMMdd}_{to:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> PurchaseReportExcel(DateTime from, DateTime to)
        {
            var bytes = await _reportService.GeneratePurchaseReportExcelAsync(from, to);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"PurchaseReport_{from:yyyyMMdd}_{to:yyyyMMdd}.xlsx");
        }

        // ---------------- Inventory ----------------

        public async Task<IActionResult> InventoryReportPdf()
        {
            var bytes = await _reportService.GenerateInventoryReportPdfAsync();
            return File(bytes, "application/pdf", $"InventoryReport_{DateTime.Today:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> InventoryReportExcel()
        {
            var bytes = await _reportService.GenerateInventoryReportExcelAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"InventoryReport_{DateTime.Today:yyyyMMdd}.xlsx");
        }

        // ---------------- Expiry ----------------

        public async Task<IActionResult> ExpiryReportPdf()
        {
            var bytes = await _reportService.GenerateExpiryReportPdfAsync();
            return File(bytes, "application/pdf", $"ExpiryReport_{DateTime.Today:yyyyMMdd}.pdf");
        }

        public async Task<IActionResult> ExpiryReportExcel()
        {
            var bytes = await _reportService.GenerateExpiryReportExcelAsync();
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"ExpiryReport_{DateTime.Today:yyyyMMdd}.xlsx");
        }
    }
}
