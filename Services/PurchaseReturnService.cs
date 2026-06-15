using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.Models.Enums;
using PharmaTrackPro.ViewModels.PurchaseReturn;

namespace PharmaTrackPro.Services
{
    public class PurchaseReturnService : IPurchaseReturnService
    {
        private readonly IPurchaseReturnRepository _purchaseReturnRepository;
        private readonly IPurchaseRepository _purchaseRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<PurchaseReturnService> _logger;

        public PurchaseReturnService(
            IPurchaseReturnRepository purchaseReturnRepository,
            IPurchaseRepository purchaseRepository,
            IBatchRepository batchRepository,
            IAuditLogService auditLogService,
            ILogger<PurchaseReturnService> logger)
        {
            _purchaseReturnRepository = purchaseReturnRepository;
            _purchaseRepository = purchaseRepository;
            _batchRepository = batchRepository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<PurchaseReturn>> GetAllAsync() => await _purchaseReturnRepository.GetAllWithDetailsAsync();

        public async Task<PurchaseReturn?> GetByIdAsync(int id) => await _purchaseReturnRepository.GetByIdWithDetailsAsync(id);

        public async Task<IEnumerable<Batch>> GetReturnableBatchesAsync(int purchaseId)
        {
            var batches = await _batchRepository.GetByPurchaseIdAsync(purchaseId);
            return batches.Where(b => b.QuantityRemaining > 0);
        }

        public async Task<ServiceResult<int>> CreateAsync(PurchaseReturnFormViewModel model, string createdByUserId)
        {
            var purchase = await _purchaseRepository.GetByIdAsync(model.PurchaseId);
            if (purchase is null)
            {
                return ServiceResult<int>.Fail("Purchase not found.");
            }

            if (purchase.Status != PurchaseStatus.Received)
            {
                return ServiceResult<int>.Fail("Only received purchases can have a return created against them.");
            }

            if (model.Items is null || model.Items.Count == 0)
            {
                return ServiceResult<int>.Fail("Enter a return quantity for at least one item.");
            }

            var returnableBatches = (await _batchRepository.GetByPurchaseIdAsync(model.PurchaseId))
                .ToDictionary(b => b.Id);

            var purchaseReturn = new PurchaseReturn
            {
                PurchaseId = model.PurchaseId,
                ReturnDate = model.ReturnDate,
                Reason = model.Reason.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            decimal totalRefund = 0;
            var batchesToUpdate = new List<Batch>();

            foreach (var item in model.Items)
            {
                if (!returnableBatches.TryGetValue(item.BatchId, out var batch))
                {
                    return ServiceResult<int>.Fail("One of the selected batches does not belong to this purchase.");
                }

                if (item.Quantity <= 0)
                {
                    continue; // rows left at 0 are simply skipped, not errors
                }

                if (item.Quantity > batch.QuantityRemaining)
                {
                    return ServiceResult<int>.Fail(
                        $"Cannot return {item.Quantity} units of \"{batch.Medicine?.Name}\" (batch {batch.BatchNumber}) — only {batch.QuantityRemaining} remain in stock.");
                }

                purchaseReturn.Items.Add(new PurchaseReturnItem
                {
                    BatchId = batch.Id,
                    Quantity = item.Quantity,
                    UnitCost = batch.UnitCost
                });

                totalRefund += item.Quantity * batch.UnitCost;
                batch.QuantityRemaining -= item.Quantity;
                batchesToUpdate.Add(batch);
            }

            if (purchaseReturn.Items.Count == 0)
            {
                return ServiceResult<int>.Fail("Enter a return quantity greater than zero for at least one item.");
            }

            purchaseReturn.TotalRefundAmount = totalRefund;

            foreach (var batch in batchesToUpdate)
            {
                _batchRepository.Update(batch);
            }

            await _purchaseReturnRepository.AddAsync(purchaseReturn);
            await _purchaseReturnRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Purchase return #{Id} created against Purchase #{PurchaseId} (Refund: {Refund:C}).",
                purchaseReturn.Id, model.PurchaseId, totalRefund);

            await _auditLogService.LogAsync(
                "PurchaseReturnCreated",
                "PurchaseReturn",
                purchaseReturn.Id.ToString(),
                $"Purchase return #{purchaseReturn.Id} created against Purchase #{model.PurchaseId} — Refund: {totalRefund:C}.");

            return ServiceResult<int>.Ok(purchaseReturn.Id, "Purchase return recorded successfully.");
        }
    }
}
