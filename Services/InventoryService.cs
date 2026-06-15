using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Inventory;

namespace PharmaTrackPro.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly ISettingsService _settingsService;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(
            IMedicineRepository medicineRepository,
            IBatchRepository batchRepository,
            ISettingsService settingsService,
            IAuditLogService auditLogService,
            ILogger<InventoryService> logger)
        {
            _medicineRepository = medicineRepository;
            _batchRepository = batchRepository;
            _settingsService = settingsService;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<InventoryItemViewModel>> GetInventorySummaryAsync()
        {
            var settings = await _settingsService.GetSettingsAsync();
            var lowStockThreshold = settings.LowStockThreshold;
            var nearExpiryDays = settings.NearExpiryDaysThreshold;
            var today = DateTime.Today;

            var medicines = (await _medicineRepository.GetAllWithDetailsAsync()).Where(m => m.IsActive);
            var batches = await _batchRepository.GetAllWithMedicineAsync();
            var batchesByMedicine = batches.GroupBy(b => b.MedicineId).ToDictionary(g => g.Key, g => g.ToList());

            var result = new List<InventoryItemViewModel>();

            foreach (var medicine in medicines)
            {
                batchesByMedicine.TryGetValue(medicine.Id, out var medicineBatches);
                medicineBatches ??= new List<Batch>();

                var totalQuantity = medicineBatches.Sum(b => b.QuantityRemaining);

                var hasExpired = medicineBatches.Any(b => b.QuantityRemaining > 0 && b.ExpiryDate.Date < today);

                var hasNearExpiry = medicineBatches.Any(b =>
                    b.QuantityRemaining > 0 &&
                    b.ExpiryDate.Date >= today &&
                    (b.ExpiryDate.Date - today).TotalDays <= nearExpiryDays);

                result.Add(new InventoryItemViewModel
                {
                    MedicineId = medicine.Id,
                    Name = medicine.Name,
                    Strength = medicine.Strength,
                    CategoryName = medicine.Category?.Name ?? "—",
                    ManufacturerName = medicine.Manufacturer?.Name ?? "—",
                    UnitOfMeasure = medicine.UnitOfMeasure.ToString(),
                    TotalQuantity = totalQuantity,
                    SellingPrice = medicine.SellingPrice,
                    IsLowStock = totalQuantity <= lowStockThreshold,
                    HasExpiredBatches = hasExpired,
                    HasNearExpiryBatches = hasNearExpiry
                });
            }

            return result.OrderBy(i => i.Name).ToList();
        }

        public async Task<IEnumerable<Batch>> GetBatchesForMedicineAsync(int medicineId) =>
            await _batchRepository.GetByMedicineIdAsync(medicineId);

        public async Task<ServiceResult> AdjustBatchQuantityAsync(int batchId, int newQuantity, string reason, string userId)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return ServiceResult.Fail("A reason is required for stock adjustments.");
            }

            var batch = await _batchRepository.GetByIdAsync(batchId);
            if (batch is null)
            {
                return ServiceResult.Fail("Batch not found.");
            }

            if (newQuantity < 0)
            {
                return ServiceResult.Fail("Quantity cannot be negative.");
            }

            if (newQuantity > batch.QuantityReceived)
            {
                return ServiceResult.Fail($"Adjusted quantity cannot exceed the originally received amount ({batch.QuantityReceived}).");
            }

            var oldQuantity = batch.QuantityRemaining;
            batch.QuantityRemaining = newQuantity;

            _batchRepository.Update(batch);
            await _batchRepository.SaveChangesAsync();

            // Permanent, queryable record now lives in AuditLogs; Serilog line stays for ops-level tailing.
            _logger.LogInformation(
                "Stock adjustment: Batch #{BatchId} ({BatchNumber}) quantity changed from {OldQuantity} to {NewQuantity} by user {UserId}. Reason: {Reason}",
                batch.Id, batch.BatchNumber, oldQuantity, newQuantity, userId, reason);

            await _auditLogService.LogAsync(
                "StockAdjustment",
                "Batch",
                batch.Id.ToString(),
                $"Batch '{batch.BatchNumber}' quantity changed from {oldQuantity} to {newQuantity}. Reason: {reason}");

            return ServiceResult.Ok("Stock adjusted successfully.");
        }
    }
}
