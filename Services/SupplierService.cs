using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Supplier;

namespace PharmaTrackPro.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly ISupplierRepository _supplierRepository;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(ISupplierRepository supplierRepository, ILogger<SupplierService> logger)
        {
            _supplierRepository = supplierRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Supplier>> GetAllAsync(bool includeDeleted = false) =>
            includeDeleted
                ? await _supplierRepository.GetAllIncludingDeletedAsync()
                : await _supplierRepository.GetAllAsync();

        public async Task<Supplier?> GetByIdAsync(int id) => await _supplierRepository.GetByIdAsync(id);

        public async Task<ServiceResult> CreateAsync(SupplierFormViewModel model)
        {
            if (await _supplierRepository.NameExistsAsync(model.Name))
            {
                return ServiceResult.Fail($"A supplier named \"{model.Name}\" already exists.");
            }

            var supplier = new Supplier
            {
                Name = model.Name.Trim(),
                ContactPerson = model.ContactPerson?.Trim(),
                Phone = model.Phone?.Trim(),
                Email = model.Email?.Trim(),
                Address = model.Address?.Trim(),
                LicenseNumber = model.LicenseNumber?.Trim(),
                PaymentTerms = model.PaymentTerms?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _supplierRepository.AddAsync(supplier);
            await _supplierRepository.SaveChangesAsync();

            _logger.LogInformation("Supplier '{Name}' created (Id: {Id}).", supplier.Name, supplier.Id);
            return ServiceResult.Ok("Supplier created successfully.");
        }

        public async Task<ServiceResult> UpdateAsync(SupplierFormViewModel model)
        {
            var supplier = await _supplierRepository.GetByIdAsync(model.Id);
            if (supplier is null)
            {
                return ServiceResult.Fail("Supplier not found.");
            }

            if (await _supplierRepository.NameExistsAsync(model.Name, model.Id))
            {
                return ServiceResult.Fail($"A supplier named \"{model.Name}\" already exists.");
            }

            supplier.Name = model.Name.Trim();
            supplier.ContactPerson = model.ContactPerson?.Trim();
            supplier.Phone = model.Phone?.Trim();
            supplier.Email = model.Email?.Trim();
            supplier.Address = model.Address?.Trim();
            supplier.LicenseNumber = model.LicenseNumber?.Trim();
            supplier.PaymentTerms = model.PaymentTerms?.Trim();
            supplier.IsActive = model.IsActive;
            supplier.UpdatedAt = DateTime.UtcNow;

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();

            _logger.LogInformation("Supplier '{Name}' (Id: {Id}) updated.", supplier.Name, supplier.Id);
            return ServiceResult.Ok("Supplier updated successfully.");
        }

        public async Task<ServiceResult> SoftDeleteAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdAsync(id);
            if (supplier is null)
            {
                return ServiceResult.Fail("Supplier not found.");
            }

            supplier.IsDeleted = true;
            supplier.DeletedAt = DateTime.UtcNow;
            supplier.IsActive = false;

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();

            _logger.LogInformation("Supplier '{Name}' (Id: {Id}) soft-deleted.", supplier.Name, supplier.Id);
            return ServiceResult.Ok("Supplier deleted. You can restore it from the deleted items view.");
        }

        public async Task<ServiceResult> RestoreAsync(int id)
        {
            var supplier = await _supplierRepository.GetByIdIncludingDeletedAsync(id);
            if (supplier is null)
            {
                return ServiceResult.Fail("Supplier not found.");
            }

            if (await _supplierRepository.NameExistsAsync(supplier.Name, supplier.Id))
            {
                return ServiceResult.Fail($"Cannot restore: an active supplier named \"{supplier.Name}\" already exists. Rename one of them first.");
            }

            supplier.IsDeleted = false;
            supplier.DeletedAt = null;
            supplier.IsActive = true;

            _supplierRepository.Update(supplier);
            await _supplierRepository.SaveChangesAsync();

            _logger.LogInformation("Supplier '{Name}' (Id: {Id}) restored.", supplier.Name, supplier.Id);
            return ServiceResult.Ok("Supplier restored successfully.");
        }
    }
}
