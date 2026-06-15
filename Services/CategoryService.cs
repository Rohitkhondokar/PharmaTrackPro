using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Category;

namespace PharmaTrackPro.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Category>> GetAllAsync(bool includeDeleted = false) =>
            includeDeleted
                ? await _categoryRepository.GetAllIncludingDeletedAsync()
                : await _categoryRepository.GetAllAsync();

        public async Task<Category?> GetByIdAsync(int id) => await _categoryRepository.GetByIdAsync(id);

        public async Task<ServiceResult> CreateAsync(CategoryFormViewModel model)
        {
            if (await _categoryRepository.NameExistsAsync(model.Name))
            {
                return ServiceResult.Fail($"A category named \"{model.Name}\" already exists.");
            }

            var category = new Category
            {
                Name = model.Name.Trim(),
                Description = model.Description?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category '{Name}' created (Id: {Id}).", category.Name, category.Id);
            return ServiceResult.Ok("Category created successfully.");
        }

        public async Task<ServiceResult> UpdateAsync(CategoryFormViewModel model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.Id);
            if (category is null)
            {
                return ServiceResult.Fail("Category not found.");
            }

            if (await _categoryRepository.NameExistsAsync(model.Name, model.Id))
            {
                return ServiceResult.Fail($"A category named \"{model.Name}\" already exists.");
            }

            category.Name = model.Name.Trim();
            category.Description = model.Description?.Trim();
            category.IsActive = model.IsActive;
            category.UpdatedAt = DateTime.UtcNow;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category '{Name}' (Id: {Id}) updated.", category.Name, category.Id);
            return ServiceResult.Ok("Category updated successfully.");
        }

        public async Task<ServiceResult> SoftDeleteAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category is null)
            {
                return ServiceResult.Fail("Category not found.");
            }

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            category.IsActive = false;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category '{Name}' (Id: {Id}) soft-deleted.", category.Name, category.Id);
            return ServiceResult.Ok("Category deleted. You can restore it from the deleted items view.");
        }

        public async Task<ServiceResult> RestoreAsync(int id)
        {
            var category = await _categoryRepository.GetByIdIncludingDeletedAsync(id);
            if (category is null)
            {
                return ServiceResult.Fail("Category not found.");
            }

            if (await _categoryRepository.NameExistsAsync(category.Name, category.Id))
            {
                return ServiceResult.Fail($"Cannot restore: an active category named \"{category.Name}\" already exists. Rename one of them first.");
            }

            category.IsDeleted = false;
            category.DeletedAt = null;
            category.IsActive = true;

            _categoryRepository.Update(category);
            await _categoryRepository.SaveChangesAsync();

            _logger.LogInformation("Category '{Name}' (Id: {Id}) restored.", category.Name, category.Id);
            return ServiceResult.Ok("Category restored successfully.");
        }
    }
}
