using ClosedXML.Excel;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PharmaTrackPro.Services
{
    public class ReportService : IReportService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISalesService _salesService;
        private readonly IInventoryService _inventoryService;
        private readonly IExpiryService _expiryService;

        public ReportService(
            IPurchaseRepository purchaseRepository,
            ISalesService salesService,
            IInventoryService inventoryService,
            IExpiryService expiryService)
        {
            _purchaseRepository = purchaseRepository;
            _salesService = salesService;
            _inventoryService = inventoryService;
            _expiryService = expiryService;
        }

        // ------------------------------------------------------------------
        // Sales
        // ------------------------------------------------------------------

        public async Task<byte[]> GenerateSalesReportPdfAsync(DateTime from, DateTime to)
        {
            var sales = (await _salesService.GetAllAsync())
                .Where(s => s.SaleDate.Date >= from.Date && s.SaleDate.Date <= to.Date)
                .OrderBy(s => s.SaleDate)
                .ToList();

            var headers = new[] { "Date", "Sale #", "Customer", "Payment", "Items", "Total" };
            var rows = sales.Select(s => new[]
            {
                s.SaleDate.ToString("MMM dd, yyyy"),
                $"#{s.Id}",
                s.Customer?.Name ?? "Walk-in",
                s.PaymentMethod.ToString(),
                (s.Items?.Count ?? 0).ToString(),
                s.TotalAmount.ToString("C")
            });

            var grandTotal = sales.Sum(s => s.TotalAmount);
            var footer = $"Total Sales: {sales.Count}    Grand Total: {grandTotal:C}";

            return BuildPdf("Sales Report", $"{from:MMM dd, yyyy} — {to:MMM dd, yyyy}", headers, rows, footer);
        }

        public async Task<byte[]> GenerateSalesReportExcelAsync(DateTime from, DateTime to)
        {
            var sales = (await _salesService.GetAllAsync())
                .Where(s => s.SaleDate.Date >= from.Date && s.SaleDate.Date <= to.Date)
                .OrderBy(s => s.SaleDate)
                .ToList();

            var headers = new[] { "Date", "Sale #", "Customer", "Cashier", "Payment", "Items", "Total" };
            var rows = sales.Select(s => new object[]
            {
                s.SaleDate,
                s.Id,
                s.Customer?.Name ?? "Walk-in",
                s.CashierUser?.FullName ?? "—",
                s.PaymentMethod.ToString(),
                s.Items?.Count ?? 0,
                s.TotalAmount
            });

            return BuildExcel("Sales Report", headers, rows);
        }

        // ------------------------------------------------------------------
        // Purchases
        // ------------------------------------------------------------------

        public async Task<byte[]> GeneratePurchaseReportPdfAsync(DateTime from, DateTime to)
        {
            var purchases = (await _purchaseRepository.GetAllWithDetailsAsync())
                .Where(p => p.PurchaseDate.Date >= from.Date && p.PurchaseDate.Date <= to.Date)
                .OrderBy(p => p.PurchaseDate)
                .ToList();

            var headers = new[] { "Date", "PO #", "Supplier", "Status", "Total" };
            var rows = purchases.Select(p => new[]
            {
                p.PurchaseDate.ToString("MMM dd, yyyy"),
                $"#{p.Id}",
                p.Supplier?.Name ?? "—",
                p.Status.ToString(),
                p.TotalAmount.ToString("C")
            });

            var grandTotal = purchases.Where(p => p.Status != PurchaseStatus.Cancelled).Sum(p => p.TotalAmount);
            var footer = $"Total Orders: {purchases.Count}    Grand Total (excl. cancelled): {grandTotal:C}";

            return BuildPdf("Purchase Report", $"{from:MMM dd, yyyy} — {to:MMM dd, yyyy}", headers, rows, footer);
        }

        public async Task<byte[]> GeneratePurchaseReportExcelAsync(DateTime from, DateTime to)
        {
            var purchases = (await _purchaseRepository.GetAllWithDetailsAsync())
                .Where(p => p.PurchaseDate.Date >= from.Date && p.PurchaseDate.Date <= to.Date)
                .OrderBy(p => p.PurchaseDate)
                .ToList();

            var headers = new[] { "Date", "PO #", "Supplier", "Invoice #", "Status", "Total" };
            var rows = purchases.Select(p => new object[]
            {
                p.PurchaseDate,
                p.Id,
                p.Supplier?.Name ?? "—",
                p.InvoiceNumber ?? "",
                p.Status.ToString(),
                p.TotalAmount
            });

            return BuildExcel("Purchase Report", headers, rows);
        }

        // ------------------------------------------------------------------
        // Inventory (point-in-time snapshot)
        // ------------------------------------------------------------------

        public async Task<byte[]> GenerateInventoryReportPdfAsync()
        {
            var items = (await _inventoryService.GetInventorySummaryAsync()).ToList();

            var headers = new[] { "Medicine", "Category", "Manufacturer", "Unit", "In Stock", "Selling Price" };
            var rows = items.Select(i => new[]
            {
                i.Name + (string.IsNullOrEmpty(i.Strength) ? "" : $" ({i.Strength})"),
                i.CategoryName,
                i.ManufacturerName,
                i.UnitOfMeasure,
                i.TotalQuantity.ToString() + (i.IsLowStock ? " (Low)" : ""),
                i.SellingPrice.ToString("C")
            });

            var footer = $"Total Medicines: {items.Count}    Low Stock: {items.Count(i => i.IsLowStock)}";

            return BuildPdf("Inventory Report", $"As of {DateTime.Today:MMM dd, yyyy}", headers, rows, footer);
        }

        public async Task<byte[]> GenerateInventoryReportExcelAsync()
        {
            var items = (await _inventoryService.GetInventorySummaryAsync()).ToList();

            var headers = new[] { "Medicine", "Strength", "Category", "Manufacturer", "Unit", "In Stock", "Low Stock", "Near Expiry", "Expired", "Selling Price" };
            var rows = items.Select(i => new object[]
            {
                i.Name,
                i.Strength ?? "",
                i.CategoryName,
                i.ManufacturerName,
                i.UnitOfMeasure,
                i.TotalQuantity,
                i.IsLowStock ? "Yes" : "No",
                i.HasNearExpiryBatches ? "Yes" : "No",
                i.HasExpiredBatches ? "Yes" : "No",
                i.SellingPrice
            });

            return BuildExcel("Inventory Report", headers, rows);
        }

        // ------------------------------------------------------------------
        // Expiry (point-in-time snapshot)
        // ------------------------------------------------------------------

        public async Task<byte[]> GenerateExpiryReportPdfAsync()
        {
            var report = (await _expiryService.GetExpiryReportAsync()).ToList();

            var headers = new[] { "Medicine", "Batch #", "Expiry Date", "Status", "Qty Remaining", "Unit Cost" };
            var rows = report.Select(b => new[]
            {
                b.MedicineName + (string.IsNullOrEmpty(b.Strength) ? "" : $" ({b.Strength})"),
                b.BatchNumber,
                b.ExpiryDate.ToString("MMM dd, yyyy"),
                b.IsExpired ? $"Expired {Math.Abs(b.DaysUntilExpiry)}d ago" : $"{b.DaysUntilExpiry}d left",
                b.QuantityRemaining.ToString(),
                b.UnitCost.ToString("C")
            });

            var footer = $"Expired Batches: {report.Count(b => b.IsExpired)}    Total Batches Listed: {report.Count}";

            return BuildPdf("Expiry Report", $"As of {DateTime.Today:MMM dd, yyyy}", headers, rows, footer);
        }

        public async Task<byte[]> GenerateExpiryReportExcelAsync()
        {
            var report = (await _expiryService.GetExpiryReportAsync()).ToList();

            var headers = new[] { "Medicine", "Strength", "Batch #", "Expiry Date", "Days Until Expiry", "Expired", "Qty Remaining", "Unit Cost" };
            var rows = report.Select(b => new object[]
            {
                b.MedicineName,
                b.Strength ?? "",
                b.BatchNumber,
                b.ExpiryDate,
                b.DaysUntilExpiry,
                b.IsExpired ? "Yes" : "No",
                b.QuantityRemaining,
                b.UnitCost
            });

            return BuildExcel("Expiry Report", headers, rows);
        }

        // ------------------------------------------------------------------
        // Shared PDF / Excel builders
        // ------------------------------------------------------------------

        private static byte[] BuildPdf(string title, string subtitle, string[] headers, IEnumerable<string[]> rows, string? footerSummary)
        {
            var rowList = rows.ToList();

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().Text("PharmaTrack Pro").FontSize(16).Bold();
                        col.Item().Text(title).FontSize(13).SemiBold();
                        col.Item().Text(subtitle).FontSize(9).FontColor(Colors.Grey.Darken1);
                    });

                    page.Content().PaddingTop(15).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            foreach (var _ in headers)
                            {
                                columns.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (var h in headers)
                            {
                                header.Cell().Border(1).BorderColor(Colors.Grey.Lighten1)
                                    .Background(Colors.Grey.Lighten3).Padding(4)
                                    .Text(h).Bold();
                            }
                        });

                        foreach (var row in rowList)
                        {
                            foreach (var cell in row)
                            {
                                table.Cell().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(cell);
                            }
                        }
                    });

                    page.Footer().Column(col =>
                    {
                        if (!string.IsNullOrEmpty(footerSummary))
                        {
                            col.Item().PaddingBottom(4).Text(footerSummary).FontSize(9).SemiBold();
                        }
                        col.Item().AlignCenter().Text(x =>
                        {
                            x.Span("Generated ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.Span(DateTime.Now.ToString("MMM dd, yyyy HH:mm")).FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.Span(" — Page ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.Span(" of ").FontSize(8).FontColor(Colors.Grey.Darken1);
                            x.TotalPages().FontSize(8).FontColor(Colors.Grey.Darken1);
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static byte[] BuildExcel(string sheetName, string[] headers, IEnumerable<object[]> rows)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            for (int c = 0; c < headers.Length; c++)
            {
                var cell = worksheet.Cell(1, c + 1);
                cell.Value = headers[c];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E9ECEF");
            }

            int rowIndex = 2;
            foreach (var row in rows)
            {
                for (int c = 0; c < row.Length; c++)
                {
                    var cell = worksheet.Cell(rowIndex, c + 1);
                    SetCellValue(cell, row[c]);
                }
                rowIndex++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static void SetCellValue(IXLCell cell, object value)
        {
            switch (value)
            {
                case DateTime dt:
                    cell.Value = dt;
                    cell.Style.DateFormat.Format = "yyyy-mm-dd";
                    break;
                case decimal dec:
                    cell.Value = dec;
                    cell.Style.NumberFormat.Format = "$#,##0.00";
                    break;
                case int i:
                    cell.Value = i;
                    break;
                default:
                    cell.Value = value?.ToString() ?? "";
                    break;
            }
        }
    }
}
