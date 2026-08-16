using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedicalStore.Business.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CustomerService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<CustomerVM>> GetAllAsync()
        {
            var customers = await _unitOfWork.Customers.GetAllAsync();
            return _mapper.Map<IEnumerable<CustomerVM>>(customers);
        }

        public async Task<CustomerVM?> GetByIdAsync(int id)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(id);
            return customer == null ? null : _mapper.Map<CustomerVM>(customer);
        }

        public async Task<int> CreateAsync(CustomerVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Check for duplicate email
            if (!string.IsNullOrWhiteSpace(model.CustEmail) && await IsEmailDuplicateAsync(model.CustEmail))
                throw new InvalidOperationException($"Customer with email '{model.CustEmail}' already exists.");

            var entity = _mapper.Map<Customer>(model);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;
            entity.CustBal = 0;

            await _unitOfWork.Customers.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer created: {CustName} (ID: {CustId})", entity.CustName, entity.CustId);
            return entity.CustId;
        }

        public async Task<bool> UpdateAsync(CustomerVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entity = await _unitOfWork.Customers.GetByIdAsync(model.CustId)
                ?? throw new KeyNotFoundException($"Customer with ID {model.CustId} not found.");

            // Check for duplicate email (excluding current customer)
            if (!string.IsNullOrWhiteSpace(model.CustEmail) && await IsEmailDuplicateAsync(model.CustEmail, model.CustId))
                throw new InvalidOperationException($"Customer with email '{model.CustEmail}' already exists.");

            // Preserve balance - balance is managed by Sale/Receipt, not direct edit
            var existingBal = entity.CustBal;
            _mapper.Map(model, entity);
            entity.CustBal = existingBal;
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Customers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer updated: {CustName} (ID: {CustId})", entity.CustName, model.CustId);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Customers.GetByIdAsync(id);
            if (entity == null) return false;

            // Check if customer has any sales
            var sales = await _unitOfWork.Sales.FindAsync(s => s.CustId == id);
            if (sales.Any())
                throw new InvalidOperationException($"Cannot delete customer '{entity.CustName}' as they have {sales.Count()} sale(s).");

            // Check if customer has any receipts
            var receipts = await _unitOfWork.Receipts.FindAsync(r => r.CustId == id);
            if (receipts.Any())
                throw new InvalidOperationException($"Cannot delete customer '{entity.CustName}' as they have {receipts.Count()} receipt(s).");

            _unitOfWork.Customers.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Customer deleted: {CustName} (ID: {CustId})", entity.CustName, id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Customers.GetByIdAsync(id) != null;
        }

        public async Task<bool> IsEmailDuplicateAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;

            var allCustomers = await _unitOfWork.Customers.FindAsync(
                c => c.CustEmail != null && c.CustEmail.ToLower() == email.ToLower());

            return excludeId.HasValue
                ? allCustomers.Any(c => c.CustId != excludeId.Value)
                : allCustomers.Any();
        }

        public async Task<decimal> GetBalanceAsync(int customerId)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            return customer?.CustBal ?? 0;
        }

        public async Task<bool> UpdateBalanceAsync(int customerId, decimal amount, bool isAddition)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId);
            if (customer == null) return false;

            if (isAddition)
                customer.CustBal += amount;
            else
                customer.CustBal -= amount;

            _unitOfWork.Customers.Update(customer);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<CustomerVM>> GetCustomersWithOutstandingBalance()
        {
            var customers = await _unitOfWork.Customers.FindAsync(c => c.CustBal > 0 && c.IsActive);
            return _mapper.Map<IEnumerable<CustomerVM>>(customers);
        }

        public async Task<IEnumerable<CustomerVM>> GetActiveCustomersAsync()
        {
            var customers = await _unitOfWork.Customers.FindAsync(c => c.IsActive);
            return _mapper.Map<IEnumerable<CustomerVM>>(customers);
        }
    }
}