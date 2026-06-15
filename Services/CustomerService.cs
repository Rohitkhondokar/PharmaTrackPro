using PharmaTrackPro.Helpers;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models;
using PharmaTrackPro.ViewModels.Customer;

namespace PharmaTrackPro.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(ICustomerRepository customerRepository, ILogger<CustomerService> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(bool includeDeleted = false) =>
            includeDeleted
                ? await _customerRepository.GetAllIncludingDeletedAsync()
                : await _customerRepository.GetAllAsync();

        public async Task<Customer?> GetByIdAsync(int id) => await _customerRepository.GetByIdAsync(id);

        public async Task<ServiceResult> CreateAsync(CustomerFormViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.Phone) && await _customerRepository.PhoneExistsAsync(model.Phone))
            {
                return ServiceResult.Fail($"A customer with phone number \"{model.Phone}\" already exists.");
            }

            var customer = new Customer
            {
                Name = model.Name.Trim(),
                Phone = model.Phone?.Trim(),
                Email = model.Email?.Trim(),
                Address = model.Address?.Trim(),
                DateOfBirth = model.DateOfBirth,
                MedicalNotes = model.MedicalNotes?.Trim(),
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Customer '{Name}' created (Id: {Id}).", customer.Name, customer.Id);
            return ServiceResult.Ok("Customer created successfully.");
        }

        public async Task<ServiceResult> UpdateAsync(CustomerFormViewModel model)
        {
            var customer = await _customerRepository.GetByIdAsync(model.Id);
            if (customer is null)
            {
                return ServiceResult.Fail("Customer not found.");
            }

            if (!string.IsNullOrWhiteSpace(model.Phone) && await _customerRepository.PhoneExistsAsync(model.Phone, model.Id))
            {
                return ServiceResult.Fail($"A customer with phone number \"{model.Phone}\" already exists.");
            }

            customer.Name = model.Name.Trim();
            customer.Phone = model.Phone?.Trim();
            customer.Email = model.Email?.Trim();
            customer.Address = model.Address?.Trim();
            customer.DateOfBirth = model.DateOfBirth;
            customer.MedicalNotes = model.MedicalNotes?.Trim();
            customer.IsActive = model.IsActive;
            customer.UpdatedAt = DateTime.UtcNow;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Customer '{Name}' (Id: {Id}) updated.", customer.Name, customer.Id);
            return ServiceResult.Ok("Customer updated successfully.");
        }

        public async Task<ServiceResult> SoftDeleteAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer is null)
            {
                return ServiceResult.Fail("Customer not found.");
            }

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.IsActive = false;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Customer '{Name}' (Id: {Id}) soft-deleted.", customer.Name, customer.Id);
            return ServiceResult.Ok("Customer deleted. You can restore it from the deleted items view.");
        }

        public async Task<ServiceResult> RestoreAsync(int id)
        {
            var customer = await _customerRepository.GetByIdIncludingDeletedAsync(id);
            if (customer is null)
            {
                return ServiceResult.Fail("Customer not found.");
            }

            customer.IsDeleted = false;
            customer.DeletedAt = null;
            customer.IsActive = true;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Customer '{Name}' (Id: {Id}) restored.", customer.Name, customer.Id);
            return ServiceResult.Ok("Customer restored successfully.");
        }
    }
}
