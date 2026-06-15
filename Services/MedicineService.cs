using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Medicine;

namespace PharmaTrackPro.Services
{
    public class MedicineService : IMedicineService
    {
        private readonly IMedicineRepository _medicineRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IManufacturerRepository _manufacturerRepository;
        private readonly IBarcodeGeneratorService _barcodeGenerator;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<MedicineService> _logger;

        public MedicineService(
            IMedicineRepository medicineRepository,
            ICategoryRepository categoryRepository,
            IManufacturerRepository manufacturerRepository,
            IBarcodeGeneratorService barcodeGenerator,
            IWebHostEnvironment env,
            ILogger<MedicineService> logger)
        {
            _medicineRepository = medicineRepository;
            _categoryRepository = categoryRepository;
            _manufacturerRepository = manufacturerRepository;
            _barcodeGenerator = barcodeGenerator;
            _env = env;
            _logger = logger;
        }

        public async Task<IEnumerable<Medicine>> GetAllAsync(bool includeDeleted = false) =>
            await _medicineRepository.GetAllWithDetailsAsync(includeDeleted);

        public async Task<Medicine?> GetByIdAsync(int id) => await _medicineRepository.GetByIdWithDetailsAsync(id);

        public async Task<ServiceResult> CreateAsync(MedicineFormViewModel model)
        {
            var validation = await ValidateAsync(model);
            if (!validation.Success)
            {
                return validation;
            }

            string? imagePath = null;
            if (model.ImageFile is not null)
            {
                var uploadResult = await FileUploadHelper.SaveImageAsync(model.ImageFile, "medicines", _env);
                if (!uploadResult.Success)
                {
                    return ServiceResult.Fail(uploadResult.Error!);
                }
                imagePath = uploadResult.RelativePath;
            }

            var medicine = new Medicine
            {
                Name = model.Name.Trim(),
                GenericName = model.GenericName?.Trim(),
                CategoryId = model.CategoryId,
                ManufacturerId = model.ManufacturerId,
                Strength = model.Strength?.Trim(),
                UnitOfMeasure = model.UnitOfMeasure,
                PurchasePrice = model.PurchasePrice,
                SellingPrice = model.SellingPrice,
                RequiresPrescription = model.RequiresPrescription,
                IsActive = model.IsActive,
                ImagePath = imagePath,
                CreatedAt = DateTime.UtcNow
            };

            await _medicineRepository.AddAsync(medicine);
            await _medicineRepository.SaveChangesAsync(); // now has an Id

            await AssignBarcodeAndQrAsync(medicine);

            _medicineRepository.Update(medicine);
            await _medicineRepository.SaveChangesAsync();

            _logger.LogInformation("Medicine '{Name}' created (Id: {Id}, Barcode: {Barcode}).", medicine.Name, medicine.Id, medicine.Barcode);
            return ServiceResult.Ok("Medicine created successfully.");
        }

        public async Task<ServiceResult> UpdateAsync(MedicineFormViewModel model)
        {
            var medicine = await _medicineRepository.GetByIdAsync(model.Id);
            if (medicine is null)
            {
                return ServiceResult.Fail("Medicine not found.");
            }

            var validation = await ValidateAsync(model);
            if (!validation.Success)
            {
                return validation;
            }

            if (model.ImageFile is not null)
            {
                var uploadResult = await FileUploadHelper.SaveImageAsync(model.ImageFile, "medicines", _env);
                if (!uploadResult.Success)
                {
                    return ServiceResult.Fail(uploadResult.Error!);
                }

                FileUploadHelper.DeleteIfExists(medicine.ImagePath, _env);
                medicine.ImagePath = uploadResult.RelativePath;
            }

            medicine.Name = model.Name.Trim();
            medicine.GenericName = model.GenericName?.Trim();
            medicine.CategoryId = model.CategoryId;
            medicine.ManufacturerId = model.ManufacturerId;
            medicine.Strength = model.Strength?.Trim();
            medicine.UnitOfMeasure = model.UnitOfMeasure;
            medicine.PurchasePrice = model.PurchasePrice;
            medicine.SellingPrice = model.SellingPrice;
            medicine.RequiresPrescription = model.RequiresPrescription;
            medicine.IsActive = model.IsActive;
            medicine.UpdatedAt = DateTime.UtcNow;

            _medicineRepository.Update(medicine);
            await _medicineRepository.SaveChangesAsync();

            _logger.LogInformation("Medicine '{Name}' (Id: {Id}) updated.", medicine.Name, medicine.Id);
            return ServiceResult.Ok("Medicine updated successfully.");
        }

        public async Task<ServiceResult> SoftDeleteAsync(int id)
        {
            var medicine = await _medicineRepository.GetByIdAsync(id);
            if (medicine is null)
            {
                return ServiceResult.Fail("Medicine not found.");
            }

            medicine.IsDeleted = true;
            medicine.DeletedAt = DateTime.UtcNow;
            medicine.IsActive = false;

            _medicineRepository.Update(medicine);
            await _medicineRepository.SaveChangesAsync();

            _logger.LogInformation("Medicine '{Name}' (Id: {Id}) soft-deleted.", medicine.Name, medicine.Id);
            return ServiceResult.Ok("Medicine deleted. You can restore it from the deleted items view.");
        }

        public async Task<ServiceResult> RestoreAsync(int id)
        {
            var medicine = await _medicineRepository.GetByIdIncludingDeletedAsync(id);
            if (medicine is null)
            {
                return ServiceResult.Fail("Medicine not found.");
            }

            if (await _medicineRepository.NameStrengthExistsAsync(medicine.Name, medicine.Strength, medicine.Id))
            {
                return ServiceResult.Fail($"Cannot restore: an active medicine matching \"{medicine.Name}\" ({medicine.Strength}) already exists.");
            }

            medicine.IsDeleted = false;
            medicine.DeletedAt = null;
            medicine.IsActive = true;

            _medicineRepository.Update(medicine);
            await _medicineRepository.SaveChangesAsync();

            _logger.LogInformation("Medicine '{Name}' (Id: {Id}) restored.", medicine.Name, medicine.Id);
            return ServiceResult.Ok("Medicine restored successfully.");
        }

        /// <summary>
        /// Generates a unique barcode value plus barcode/QR PNG images for a
        /// newly-created medicine and attaches the resulting paths to it.
        /// Called once, right after the medicine's first save (needs its Id
        /// for the image filenames). Does not persist — caller saves afterward.
        /// </summary>
        private async Task AssignBarcodeAndQrAsync(Medicine medicine)
        {
            string value;
            var attempts = 0;
            do
            {
                value = _barcodeGenerator.GenerateCandidateValue();
                attempts++;
            }
            while (await _medicineRepository.BarcodeExistsAsync(value) && attempts < 10);

            var barcodeBytes = _barcodeGenerator.GenerateBarcodeImage(value);
            var qrBytes = _barcodeGenerator.GenerateQrCodeImage(value);

            var fileName = $"med-{medicine.Id}.png";

            medicine.Barcode = value;
            medicine.BarcodeImagePath = await FileUploadHelper.SaveBytesAsync(barcodeBytes, "barcodes", fileName, _env);
            medicine.QrCodeImagePath = await FileUploadHelper.SaveBytesAsync(qrBytes, "qrcodes", fileName, _env);
        }

        /// <summary>
        /// Shared validation for Create/Update: FK existence, name+strength
        /// uniqueness, and the selling-price-vs-purchase-price business rule.
        /// </summary>
        private async Task<ServiceResult> ValidateAsync(MedicineFormViewModel model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.CategoryId);
            if (category is null)
            {
                return ServiceResult.Fail("Selected category does not exist or has been deleted.");
            }

            var manufacturer = await _manufacturerRepository.GetByIdAsync(model.ManufacturerId);
            if (manufacturer is null)
            {
                return ServiceResult.Fail("Selected manufacturer does not exist or has been deleted.");
            }

            if (await _medicineRepository.NameStrengthExistsAsync(model.Name, model.Strength, model.Id > 0 ? model.Id : null))
            {
                return ServiceResult.Fail($"A medicine named \"{model.Name}\"" +
                    (string.IsNullOrWhiteSpace(model.Strength) ? "" : $" ({model.Strength})") +
                    " already exists.");
            }

            if (model.SellingPrice < model.PurchasePrice)
            {
                return ServiceResult.Fail("Selling price cannot be lower than purchase price.");
            }

            return ServiceResult.Ok(string.Empty);
        }
    }
}
