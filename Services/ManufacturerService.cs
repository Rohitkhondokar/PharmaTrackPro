using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Manufacturer;

namespace PharmaTrackPro.Services
{
    public class ManufacturerService : IManufacturerService
    {
        private readonly IManufacturerRepository _manufacturerRepository;
        private readonly ILogger<ManufacturerService> _logger;

        public ManufacturerService(IManufacturerRepository manufacturerRepository, ILogger<ManufacturerService> logger)
        {
            _manufacturerRepository = manufacturerRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Manufacturer>> GetAllAsync(bool includeDeleted = false) =>
            includeDeleted
                ? await _manufacturerRepository.GetAllIncludingDeletedAsync()
                : await _manufacturerRepository.GetAllAsync();

        public async Task<Manufacturer?> GetByIdAsync(int id) => await _manufacturerRepository.GetByIdAsync(id);

        public async Task<ServiceResult> CreateAsync(ManufacturerFormViewModel model)
        {
            if (await _manufacturerRepository.NameExistsAsync(model.Name))
            {
                return ServiceResult.Fail($"A manufacturer named \"{model.Name}\" already exists.");
            }

            var manufacturer = new Manufacturer
            {
                Name = model.Name.Trim(),
                ContactPerson = model.ContactPerson?.Trim(),
                Phone = model.Phone?.Trim(),
                Email = model.Email?.Trim(),
                Address = model.Address?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _manufacturerRepository.AddAsync(manufacturer);
            await _manufacturerRepository.SaveChangesAsync();

            _logger.LogInformation("Manufacturer '{Name}' created (Id: {Id}).", manufacturer.Name, manufacturer.Id);
            return ServiceResult.Ok("Manufacturer created successfully.");
        }

        public async Task<ServiceResult> UpdateAsync(ManufacturerFormViewModel model)
        {
            var manufacturer = await _manufacturerRepository.GetByIdAsync(model.Id);
            if (manufacturer is null)
            {
                return ServiceResult.Fail("Manufacturer not found.");
            }

            if (await _manufacturerRepository.NameExistsAsync(model.Name, model.Id))
            {
                return ServiceResult.Fail($"A manufacturer named \"{model.Name}\" already exists.");
            }

            manufacturer.Name = model.Name.Trim();
            manufacturer.ContactPerson = model.ContactPerson?.Trim();
            manufacturer.Phone = model.Phone?.Trim();
            manufacturer.Email = model.Email?.Trim();
            manufacturer.Address = model.Address?.Trim();
            manufacturer.IsActive = model.IsActive;
            manufacturer.UpdatedAt = DateTime.UtcNow;

            _manufacturerRepository.Update(manufacturer);
            await _manufacturerRepository.SaveChangesAsync();

            _logger.LogInformation("Manufacturer '{Name}' (Id: {Id}) updated.", manufacturer.Name, manufacturer.Id);
            return ServiceResult.Ok("Manufacturer updated successfully.");
        }

        public async Task<ServiceResult> SoftDeleteAsync(int id)
        {
            var manufacturer = await _manufacturerRepository.GetByIdAsync(id);
            if (manufacturer is null)
            {
                return ServiceResult.Fail("Manufacturer not found.");
            }

            manufacturer.IsDeleted = true;
            manufacturer.DeletedAt = DateTime.UtcNow;
            manufacturer.IsActive = false;

            _manufacturerRepository.Update(manufacturer);
            await _manufacturerRepository.SaveChangesAsync();

            _logger.LogInformation("Manufacturer '{Name}' (Id: {Id}) soft-deleted.", manufacturer.Name, manufacturer.Id);
            return ServiceResult.Ok("Manufacturer deleted. You can restore it from the deleted items view.");
        }

        public async Task<ServiceResult> RestoreAsync(int id)
        {
            var manufacturer = await _manufacturerRepository.GetByIdIncludingDeletedAsync(id);
            if (manufacturer is null)
            {
                return ServiceResult.Fail("Manufacturer not found.");
            }

            if (await _manufacturerRepository.NameExistsAsync(manufacturer.Name, manufacturer.Id))
            {
                return ServiceResult.Fail($"Cannot restore: an active manufacturer named \"{manufacturer.Name}\" already exists. Rename one of them first.");
            }

            manufacturer.IsDeleted = false;
            manufacturer.DeletedAt = null;
            manufacturer.IsActive = true;

            _manufacturerRepository.Update(manufacturer);
            await _manufacturerRepository.SaveChangesAsync();

            _logger.LogInformation("Manufacturer '{Name}' (Id: {Id}) restored.", manufacturer.Name, manufacturer.Id);
            return ServiceResult.Ok("Manufacturer restored successfully.");
        }
    }
}
