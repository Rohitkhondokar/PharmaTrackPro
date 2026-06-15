namespace PharmaTrackPro.Interfaces
{
    public interface IReportService
    {
        Task<byte[]> GenerateSalesReportPdfAsync(DateTime from, DateTime to);
        Task<byte[]> GenerateSalesReportExcelAsync(DateTime from, DateTime to);

        Task<byte[]> GeneratePurchaseReportPdfAsync(DateTime from, DateTime to);
        Task<byte[]> GeneratePurchaseReportExcelAsync(DateTime from, DateTime to);

        Task<byte[]> GenerateInventoryReportPdfAsync();
        Task<byte[]> GenerateInventoryReportExcelAsync();

        Task<byte[]> GenerateExpiryReportPdfAsync();
        Task<byte[]> GenerateExpiryReportExcelAsync();
    }
}
