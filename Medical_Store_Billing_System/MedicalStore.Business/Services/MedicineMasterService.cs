using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;

using MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class MedicineMasterService : IMedicineMasterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicineMasterService> _logger;

        public MedicineMasterService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MedicineMasterService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<MedicineMasterVM>> GetAllAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetAllWithCategoryAndBrandAsync();
            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }

        public async Task<MedicineMasterVM?> GetByIdAsync(int id)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdWithDetailsAsync(id);
            return medicine == null ? null : _mapper.Map<MedicineMasterVM>(medicine);
        }

        public async Task<int> CreateAsync(MedicineMasterVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (await IsMedicineNameDuplicateAsync(model.MedName))
                throw new InvalidOperationException($"Medicine '{model.MedName}' already exists.");

            var entity = _mapper.Map<MedicineMaster>(model);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;

            await _unitOfWork.Medicines.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine created: {MedName} (ID: {MedId})", entity.MedName, entity.MedId);
            return entity.MedId;
        }

        public async Task<bool> UpdateAsync(MedicineMasterVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entity = await _unitOfWork.Medicines.GetByIdAsync(model.MedId)
                ?? throw new KeyNotFoundException($"Medicine with ID {model.MedId} not found.");

            if (await IsMedicineNameDuplicateAsync(model.MedName, model.MedId))
                throw new InvalidOperationException($"Medicine '{model.MedName}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Medicines.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine updated: {MedName} (ID: {MedId})", entity.MedName, model.MedId);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Medicines.GetByIdAsync(id);
            if (entity == null) return false;

            // Check if medicine is used in any sale or purchase
            var saleDetails = await _unitOfWork.SaleDetails.FindAsync(sd => sd.MedId == id);
            if (saleDetails.Any())
                throw new InvalidOperationException($"Cannot delete medicine '{entity.MedName}' as it has {saleDetails.Count()} sale(s).");

            var purchaseDetails = await _unitOfWork.PurchaseDetails.FindAsync(pd => pd.MedId == id);
            if (purchaseDetails.Any())
                throw new InvalidOperationException($"Cannot delete medicine '{entity.MedName}' as it has {purchaseDetails.Count()} purchase(s).");

            _unitOfWork.Medicines.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine deleted: {MedName} (ID: {MedId})", entity.MedName, id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Medicines.GetByIdAsync(id) != null;
        }

        public async Task<bool> IsMedicineNameDuplicateAsync(string medicineName, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(medicineName)) return false;

            var allMedicines = await _unitOfWork.Medicines.FindAsync(
                m => m.MedName.ToLower() == medicineName.ToLower());

            return excludeId.HasValue
                ? allMedicines.Any(m => m.MedId != excludeId.Value)
                : allMedicines.Any();
        }

        public async Task<IEnumerable<MedicineMasterVM>> GetLowStockMedicinesAsync(int threshold = 50)
        {
            var medicines = await _unitOfWork.Medicines.GetLowStockMedicinesAsync(threshold);
            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }

        public async Task<IEnumerable<MedicineMasterVM>> GetByCategoryAsync(int categoryId)
        {
            var medicines = await _unitOfWork.Medicines.GetMedicinesByCategoryAsync(categoryId);
            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }

        public async Task<IEnumerable<MedicineMasterVM>> GetByBrandAsync(int brandId)
        {
            var medicines = await _unitOfWork.Medicines.GetMedicinesByBrandAsync(brandId);
            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }

        public async Task<decimal> GetCurrentStockAsync(int medicineId)
        {
            var medicine = await _unitOfWork.Medicines.GetByIdAsync(medicineId);
            return medicine?.Stock ?? 0;
        }

        public async Task<bool> UpdateStockAsync(int medicineId, decimal quantity, bool isAddition)
        {
            try
            {
                await _unitOfWork.Medicines.UpdateStockAsync(medicineId, quantity, isAddition);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update stock for medicine {MedicineId}", medicineId);
                return false;
            }
        }

        public async Task<IEnumerable<MedicineMasterVM>> GetActiveMedicinesAsync()
        {
            var medicines = await _unitOfWork.Medicines.GetActiveMedicinesAsync();
            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }

        public async Task<IEnumerable<MedicineMasterVM>> SearchMedicinesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllAsync();

            var medicines = await _unitOfWork.Medicines.FindAsync(m =>
                m.MedName.Contains(searchTerm) ||
                (m.BatchNo != null && m.BatchNo.Contains(searchTerm)));

            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }

        public async Task<IEnumerable<MedicineMasterVM>> GetExpiringMedicinesAsync(int daysThreshold = 30)
        {
            var expiryDate = DateTime.UtcNow.AddDays(daysThreshold);
            var medicines = await _unitOfWork.Medicines.FindAsync(m =>
                m.ExpiryDate.HasValue &&
                m.ExpiryDate.Value <= expiryDate &&
                m.IsActive);

            return _mapper.Map<IEnumerable<MedicineMasterVM>>(medicines);
        }
    }
}
