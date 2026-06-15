using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.SaleReturn;

namespace PharmaTrackPro.Services
{
    public class SaleReturnService : ISaleReturnService
    {
        private readonly ISaleReturnRepository _saleReturnRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<SaleReturnService> _logger;

        public SaleReturnService(
            ISaleReturnRepository saleReturnRepository,
            ISaleRepository saleRepository,
            IBatchRepository batchRepository,
            IAuditLogService auditLogService,
            ILogger<SaleReturnService> logger)
        {
            _saleReturnRepository = saleReturnRepository;
            _saleRepository = saleRepository;
            _batchRepository = batchRepository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<SaleReturn>> GetAllAsync() => await _saleReturnRepository.GetAllWithDetailsAsync();

        public async Task<SaleReturn?> GetByIdAsync(int id) => await _saleReturnRepository.GetByIdWithDetailsAsync(id);

        public async Task<Dictionary<int, int>> GetReturnedQuantitiesForSaleAsync(int saleId) =>
            await _saleReturnRepository.GetReturnedQuantitiesForSaleAsync(saleId);

        public async Task<ServiceResult<int>> CreateAsync(SaleReturnFormViewModel model, string createdByUserId)
        {
            var sale = await _saleRepository.GetByIdWithDetailsAsync(model.SaleId);
            if (sale is null)
            {
                return ServiceResult<int>.Fail("Sale not found.");
            }

            if (model.Items is null || model.Items.Count == 0)
            {
                return ServiceResult<int>.Fail("Enter a return quantity for at least one item.");
            }

            var returnedQuantities = await _saleReturnRepository.GetReturnedQuantitiesForSaleAsync(model.SaleId);
            var saleItemsById = sale.Items.ToDictionary(i => i.Id);

            var saleReturn = new SaleReturn
            {
                SaleId = model.SaleId,
                ReturnDate = model.ReturnDate,
                Reason = model.Reason.Trim(),
                CreatedByUserId = createdByUserId,
                CreatedAt = DateTime.UtcNow
            };

            decimal totalRefund = 0;
            var batchesToUpdate = new List<Batch>();

            foreach (var itemInput in model.Items)
            {
                if (itemInput.Quantity <= 0)
                {
                    continue; // rows left at 0 are simply skipped, not errors
                }

                if (!saleItemsById.TryGetValue(itemInput.SaleItemId, out var saleItem))
                {
                    return ServiceResult<int>.Fail("One of the selected items does not belong to this sale.");
                }

                var alreadyReturned = returnedQuantities.TryGetValue(saleItem.Id, out var r) ? r : 0;
                var returnable = saleItem.Quantity - alreadyReturned;

                if (itemInput.Quantity > returnable)
                {
                    return ServiceResult<int>.Fail(
                        $"Cannot return {itemInput.Quantity} units of \"{saleItem.Medicine?.Name}\" — only {returnable} eligible for return.");
                }

                saleReturn.Items.Add(new SaleReturnItem
                {
                    SaleItemId = saleItem.Id,
                    Quantity = itemInput.Quantity,
                    UnitPrice = saleItem.UnitPrice
                });

                totalRefund += itemInput.Quantity * saleItem.UnitPrice;

                if (saleItem.Batch != null)
                {
                    saleItem.Batch.QuantityRemaining += itemInput.Quantity;
                    batchesToUpdate.Add(saleItem.Batch);
                }
            }

            if (saleReturn.Items.Count == 0)
            {
                return ServiceResult<int>.Fail("Enter a return quantity greater than zero for at least one item.");
            }

            saleReturn.TotalRefundAmount = totalRefund;

            foreach (var batch in batchesToUpdate)
            {
                _batchRepository.Update(batch);
            }

            await _saleReturnRepository.AddAsync(saleReturn);
            await _saleReturnRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Sale return #{Id} created against Sale #{SaleId} (Refund: {Refund:C}).",
                saleReturn.Id, model.SaleId, totalRefund);

            await _auditLogService.LogAsync(
                "SaleReturnCreated",
                "SaleReturn",
                saleReturn.Id.ToString(),
                $"Sale return #{saleReturn.Id} created against Sale #{model.SaleId} — Refund: {totalRefund:C}.");

            return ServiceResult<int>.Ok(saleReturn.Id, "Sale return recorded successfully.");
        }
    }
}
