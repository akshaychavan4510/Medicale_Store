using AutoMapper;
using Medical_Store_Billing_System.Models;

using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.Repository.Interfaces;



namespace MedicalStore.Business.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SupplierService> _logger;

        public SupplierService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SupplierService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<SupplierVM>> GetAllAsync()
        {
            var suppliers = await _unitOfWork.Suppliers.GetAllAsync();
            return _mapper.Map<IEnumerable<SupplierVM>>(suppliers);
        }

        public async Task<SupplierVM?> GetByIdAsync(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(id);
            return supplier == null ? null : _mapper.Map<SupplierVM>(supplier);
        }

        public async Task<SupplierVM?> GetByIdWithPurchasesAsync(int id)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdWithPurchasesAsync(id);
            return supplier == null ? null : _mapper.Map<SupplierVM>(supplier);
        }

        public async Task<int> CreateAsync(SupplierVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Check duplicate email
            if (!string.IsNullOrWhiteSpace(model.SuppEmail) && await IsEmailDuplicateAsync(model.SuppEmail))
                throw new InvalidOperationException($"Supplier with email '{model.SuppEmail}' already exists.");

            var entity = _mapper.Map<Supplier>(model);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;
            entity.SuppBal = 0;

            await _unitOfWork.Suppliers.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Supplier created: {SuppName} (ID: {SuppId})", entity.SuppName, entity.SuppId);
            return entity.SuppId;
        }

        public async Task<bool> UpdateAsync(SupplierVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entity = await _unitOfWork.Suppliers.GetByIdAsync(model.SuppId)
                ?? throw new KeyNotFoundException($"Supplier with ID {model.SuppId} not found.");

            // Check duplicate email (excluding self)
            if (!string.IsNullOrWhiteSpace(model.SuppEmail) && await IsEmailDuplicateAsync(model.SuppEmail, model.SuppId))
                throw new InvalidOperationException($"Supplier with email '{model.SuppEmail}' already exists.");

            var existingBal = entity.SuppBal;
            _mapper.Map(model, entity);
            entity.SuppBal = existingBal;   // preserve balance
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Suppliers.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Supplier updated: {SuppName} (ID: {SuppId})", entity.SuppName, model.SuppId);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Suppliers.GetByIdAsync(id);
            if (entity == null) return false;

            // Check if supplier has purchases
            var purchases = await _unitOfWork.Purchases.FindAsync(p => p.SuppId == id);
            if (purchases.Any())
                throw new InvalidOperationException($"Cannot delete supplier '{entity.SuppName}' as they have {purchases.Count()} purchase(s).");

            _unitOfWork.Suppliers.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Supplier deleted: {SuppName} (ID: {SuppId})", entity.SuppName, id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
            => await _unitOfWork.Suppliers.GetByIdAsync(id) != null;

        public async Task<bool> IsEmailDuplicateAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var all = await _unitOfWork.Suppliers.FindAsync(
                s => s.SuppEmail != null && s.SuppEmail.ToLower() == email.ToLower());
            return excludeId.HasValue
                ? all.Any(s => s.SuppId != excludeId.Value)
                : all.Any();
        }

        public async Task<decimal> GetBalanceAsync(int supplierId)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId);
            return supplier?.SuppBal ?? 0;
        }

        public async Task<bool> UpdateBalanceAsync(int supplierId, decimal amount, bool isAddition)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId);
            if (supplier == null) return false;

            if (isAddition)
                supplier.SuppBal += amount;
            else
                supplier.SuppBal -= amount;

            _unitOfWork.Suppliers.Update(supplier);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<SupplierVM>> GetSuppliersWithOutstandingBalance()
        {
            var suppliers = await _unitOfWork.Suppliers.FindAsync(s => s.SuppBal > 0 && s.IsActive);
            return _mapper.Map<IEnumerable<SupplierVM>>(suppliers);
        }

        public async Task<IEnumerable<SupplierVM>> GetActiveSuppliersAsync()
        {
            var suppliers = await _unitOfWork.Suppliers.FindAsync(s => s.IsActive);
            return _mapper.Map<IEnumerable<SupplierVM>>(suppliers);
        }
    }
}
