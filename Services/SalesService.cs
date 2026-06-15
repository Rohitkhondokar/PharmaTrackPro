using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Sales;

namespace PharmaTrackPro.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IMedicineRepository _medicineRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly IBatchRepository _batchRepository;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<SalesService> _logger;

        public SalesService(
            ISaleRepository saleRepository,
            IMedicineRepository medicineRepository,
            ICustomerRepository customerRepository,
            IBatchRepository batchRepository,
            IAuditLogService auditLogService,
            ILogger<SalesService> logger)
        {
            _saleRepository = saleRepository;
            _medicineRepository = medicineRepository;
            _customerRepository = customerRepository;
            _batchRepository = batchRepository;
            _auditLogService = auditLogService;
            _logger = logger;
        }

        public async Task<IEnumerable<Sale>> GetAllAsync() => await _saleRepository.GetAllWithDetailsAsync();

        public async Task<Sale?> GetByIdAsync(int id) => await _saleRepository.GetByIdWithDetailsAsync(id);

        public async Task<PosDataViewModel> GetPosDataAsync()
        {
            var today = DateTime.Today;

            var medicines = (await _medicineRepository.GetAllWithDetailsAsync()).Where(m => m.IsActive).ToList();
            var batches = await _batchRepository.GetAllWithMedicineAsync();
            var customers = await _customerRepository.GetAllAsync();

            var sellableQtyByMedicine = batches
                .Where(b => b.ExpiryDate.Date >= today && b.QuantityRemaining > 0)
                .GroupBy(b => b.MedicineId)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.QuantityRemaining));

            return new PosDataViewModel
            {
                Customers = customers
                    .Where(c => c.IsActive)
                    .Select(c => new PosCustomerViewModel { Id = c.Id, Name = c.Name })
                    .ToList(),
                Medicines = medicines
                    .Select(m => new PosMedicineViewModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Strength = m.Strength,
                        SellingPrice = m.SellingPrice,
                        RequiresPrescription = m.RequiresPrescription,
                        AvailableQuantity = sellableQtyByMedicine.TryGetValue(m.Id, out var qty) ? qty : 0
                    })
                    .ToList()
            };
        }

        public async Task<ServiceResult<int>> CheckoutAsync(SaleCheckoutViewModel model, string cashierUserId)
        {
            var cartItems = model.Items?.Where(i => i.Quantity > 0).ToList() ?? new List<SaleCartItemViewModel>();
            if (cartItems.Count == 0)
            {
                return ServiceResult<int>.Fail("Cart is empty.");
            }

            var today = DateTime.Today;
            var medicinesById = new Dictionary<int, Medicine>();
            var requiresPrescription = false;

            // Pass 1: validate everything before touching any stock, so a
            // failure partway through never leaves stock half-deducted.
            foreach (var cartItem in cartItems)
            {
                var medicine = await _medicineRepository.GetByIdAsync(cartItem.MedicineId);
                if (medicine is null || !medicine.IsActive)
                {
                    return ServiceResult<int>.Fail("One of the items in the cart is no longer available.");
                }

                medicinesById[cartItem.MedicineId] = medicine;

                if (medicine.RequiresPrescription)
                {
                    requiresPrescription = true;
                }

                var sellableQuantity = (await _batchRepository.GetByMedicineIdAsync(cartItem.MedicineId))
                    .Where(b => b.ExpiryDate.Date >= today)
                    .Sum(b => b.QuantityRemaining);

                if (cartItem.Quantity > sellableQuantity)
                {
                    return ServiceResult<int>.Fail(
                        $"Not enough stock for \"{medicine.Name}\" — requested {cartItem.Quantity}, only {sellableQuantity} available.");
                }
            }

            if (requiresPrescription && !model.PrescriptionVerified)
            {
                return ServiceResult<int>.Fail("This sale includes a prescription-only item. Please verify the prescription before completing the sale.");
            }

            // Pass 2: build the sale and deduct stock — guaranteed to succeed
            // now that quantities have already been validated above.
            var sale = new Sale
            {
                CustomerId = model.CustomerId,
                PaymentMethod = model.PaymentMethod,
                CashierUserId = cashierUserId,
                SaleDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            decimal total = 0;

            foreach (var cartItem in cartItems)
            {
                var medicine = medicinesById[cartItem.MedicineId];

                var batches = (await _batchRepository.GetByMedicineIdAsync(cartItem.MedicineId))
                    .Where(b => b.ExpiryDate.Date >= today && b.QuantityRemaining > 0)
                    .OrderBy(b => b.ExpiryDate) // FEFO — First-Expire-First-Out
                    .ToList();

                var remaining = cartItem.Quantity;

                foreach (var batch in batches)
                {
                    if (remaining <= 0)
                    {
                        break;
                    }

                    var take = Math.Min(remaining, batch.QuantityRemaining);

                    sale.Items.Add(new SaleItem
                    {
                        MedicineId = medicine.Id,
                        BatchId = batch.Id,
                        Quantity = take,
                        UnitPrice = medicine.SellingPrice
                    });

                    batch.QuantityRemaining -= take;
                    _batchRepository.Update(batch);

                    remaining -= take;
                    total += take * medicine.SellingPrice;
                }
            }

            sale.TotalAmount = total;

            await _saleRepository.AddAsync(sale);
            await _saleRepository.SaveChangesAsync();

            _logger.LogInformation(
                "Sale #{Id} completed by {UserId} — {ItemCount} item(s), Total: {Total:C}.",
                sale.Id, cashierUserId, sale.Items.Count, total);

            await _auditLogService.LogAsync(
                "SaleCompleted",
                "Sale",
                sale.Id.ToString(),
                $"Sale #{sale.Id} completed — {sale.Items.Count} item(s), Total: {total:C}.");

            return ServiceResult<int>.Ok(sale.Id, "Sale completed successfully.");
        }
    }
}
