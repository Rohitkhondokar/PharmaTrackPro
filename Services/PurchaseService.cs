using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.Models.Enums;
using PharmaTrackPro.ViewModels.Purchase;

namespace PharmaTrackPro.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly ISupplierRepository _supplierRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(
            IPurchaseRepository purchaseRepository,
            ISupplierRepository supplierRepository,
            IMedicineRepository medicineRepository,
            IBatchRepository batchRepository,
            IAuditLogService auditLogService,
            ILogger<PurchaseService> logger)
        {
            _purchaseRepository = purchaseRepository;
            _supplierRepository = supplierRepository;
            _medicineRepository = medicineRepository;
            _batchRepository = batchRepository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<Purchase>> GetAllAsync() => await _purchaseRepository.GetAllWithDetailsAsync();

        public async Task<Purchase?> GetByIdAsync(int id) => await _purchaseRepository.GetByIdWithDetailsAsync(id);

        public async Task<ServiceResult<int>> CreateAsync(PurchaseFormViewModel model, string createdByUserId)
        {
            var supplier = await _supplierRepository.GetByIdAsync(model.SupplierId);
            if (supplier is null || !supplier.IsActive)
            {
                return ServiceResult<int>.Fail("Selected supplier does not exist or is inactive.");
            }

            if (model.Items is null || model.Items.Count == 0)
            {
                return ServiceResult<int>.Fail("Add at least one line item.");
            }

            foreach (var item in model.Items)
            {
                var medicine = await _medicineRepository.GetByIdAsync(item.MedicineId);
                if (medicine is null || !medicine.IsActive)
                {
                    return ServiceResult<int>.Fail("One of the selected medicines does not exist or is inactive.");
                }

                if (item.Quantity <= 0)
                {
                    return ServiceResult<int>.Fail($"Quantity for \"{medicine.Name}\" must be at least 1.");
                }

                if (item.ExpiryDate.Date <= DateTime.Today)
                {
                    return ServiceResult<int>.Fail($"Expiry date for \"{medicine.Name}\" must be in the future — stock that's already expired can't be received.");
                }

                if (string.IsNullOrWhiteSpace(item.BatchNumber))
                {
                    return ServiceResult<int>.Fail($"Batch number is required for \"{medicine.Name}\".");
                }
            }

            var purchase = new Purchase
            {
                SupplierId = model.SupplierId,
                InvoiceNumber = model.InvoiceNumber?.Trim(),
                PurchaseDate = model.PurchaseDate,
                Notes = model.Notes?.Trim(),
                Status = PurchaseStatus.Pending,
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow,
                TotalAmount = model.Items.Sum(i => i.Quantity * i.UnitCost)
            };

            foreach (var item in model.Items)
            {
                purchase.Items.Add(new PurchaseItem
                {
                    MedicineId = item.MedicineId,
                    Quantity = item.Quantity,
                    UnitCost = item.UnitCost,
                    BatchNumber = item.BatchNumber.Trim(),
                    ExpiryDate = item.ExpiryDate.Date
                });
            }

            await _purchaseRepository.AddAsync(purchase);
            await _purchaseRepository.SaveChangesAsync();

            _logger.LogInformation("Purchase order #{Id} created for supplier '{Supplier}' (Total: {Total:C}).",
                purchase.Id, supplier.Name, purchase.TotalAmount);

            await _auditLogService.LogAsync(
                "PurchaseCreated",
                "Purchase",
                purchase.Id.ToString(),
                $"Purchase order #{purchase.Id} created for supplier '{supplier.Name}' — Total: {purchase.TotalAmount:C}.");

            return ServiceResult<int>.Ok(purchase.Id, "Purchase order created successfully.");
        }

        public async Task<ServiceResult> ReceiveAsync(int id)
        {
            var purchase = await _purchaseRepository.GetByIdWithDetailsAsync(id);
            if (purchase is null)
            {
                return ServiceResult.Fail("Purchase order not found.");
            }

            if (purchase.Status != PurchaseStatus.Pending)
            {
                return ServiceResult.Fail("Only pending purchase orders can be marked as received.");
            }

            foreach (var item in purchase.Items)
            {
                var batch = new Batch
                {
                    MedicineId = item.MedicineId,
                    PurchaseItemId = item.Id,
                    BatchNumber = item.BatchNumber,
                    ExpiryDate = item.ExpiryDate,
                    QuantityReceived = item.Quantity,
                    QuantityRemaining = item.Quantity,
                    UnitCost = item.UnitCost,
                    CreatedAt = DateTime.UtcNow
                };

                await _batchRepository.AddAsync(batch);
            }

            purchase.Status = PurchaseStatus.Received;
            purchase.ReceivedAt = DateTime.UtcNow;

            _purchaseRepository.Update(purchase);
            await _purchaseRepository.SaveChangesAsync();

            _logger.LogInformation("Purchase order #{Id} marked as received; {Count} batch(es) created.", purchase.Id, purchase.Items.Count);

            await _auditLogService.LogAsync(
                "PurchaseReceived",
                "Purchase",
                purchase.Id.ToString(),
                $"Purchase order #{purchase.Id} marked as received; {purchase.Items.Count} batch(es) created.");

            return ServiceResult.Ok("Purchase marked as received. Stock batches have been added to inventory.");
        }

        public async Task<ServiceResult> CancelAsync(int id)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(id);
            if (purchase is null)
            {
                return ServiceResult.Fail("Purchase order not found.");
            }

            if (purchase.Status != PurchaseStatus.Pending)
            {
                return ServiceResult.Fail("Only pending purchase orders can be cancelled. A received purchase requires a Purchase Return instead.");
            }

            purchase.Status = PurchaseStatus.Cancelled;
            purchase.CancelledAt = DateTime.UtcNow;

            _purchaseRepository.Update(purchase);
            await _purchaseRepository.SaveChangesAsync();

            _logger.LogInformation("Purchase order #{Id} cancelled.", purchase.Id);

            await _auditLogService.LogAsync(
                "PurchaseCancelled",
                "Purchase",
                purchase.Id.ToString(),
                $"Purchase order #{purchase.Id} cancelled.");

            return ServiceResult.Ok("Purchase order cancelled.");
        }
    }
}
