using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class MedicineCategoryService : IMedicineCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<MedicineCategoryService> _logger;

        public MedicineCategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<MedicineCategoryService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<MedicineCategoryVM>> GetAllAsync()
        {
            var categories = await _unitOfWork.MedicineCategories.GetAllAsync();
            return _mapper.Map<IEnumerable<MedicineCategoryVM>>(categories);
        }

        public async Task<MedicineCategoryVM?> GetByIdAsync(int id)
        {
            var category = await _unitOfWork.MedicineCategories.GetByIdAsync(id);
            return category == null ? null : _mapper.Map<MedicineCategoryVM>(category);
        }

        public async Task<int> CreateAsync(MedicineCategoryVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            if (await IsCategoryNameDuplicateAsync(model.CatName))
                throw new InvalidOperationException($"Category '{model.CatName}' already exists.");

            var entity = _mapper.Map<MedicineCategory>(model);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;

            await _unitOfWork.MedicineCategories.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine category created: {CatName} (ID: {CatId})", entity.CatName, entity.CatId);
            return entity.CatId;
        }

        public async Task<bool> UpdateAsync(MedicineCategoryVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entity = await _unitOfWork.MedicineCategories.GetByIdAsync(model.CatId)
                ?? throw new KeyNotFoundException($"Category with ID {model.CatId} not found.");

            if (await IsCategoryNameDuplicateAsync(model.CatName, model.CatId))
                throw new InvalidOperationException($"Category '{model.CatName}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.MedicineCategories.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine category updated: {CatName} (ID: {CatId})", entity.CatName, model.CatId);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.MedicineCategories.GetByIdAsync(id);
            if (entity == null) return false;

            // Check if category has medicines
            var medicines = await _unitOfWork.Medicines.FindAsync(m => m.CatId == id);
            if (medicines.Any())
                throw new InvalidOperationException($"Cannot delete category '{entity.CatName}' as it has {medicines.Count()} medicine(s).");

            _unitOfWork.MedicineCategories.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Medicine category deleted: {CatName} (ID: {CatId})", entity.CatName, id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.MedicineCategories.GetByIdAsync(id) != null;
        }

        public async Task<bool> IsCategoryNameDuplicateAsync(string categoryName, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) return false;

            var allCategories = await _unitOfWork.MedicineCategories.FindAsync(
                c => c.CatName.ToLower() == categoryName.ToLower());

            return excludeId.HasValue
                ? allCategories.Any(c => c.CatId != excludeId.Value)
                : allCategories.Any();
        }

        public async Task<IEnumerable<MedicineCategoryVM>> GetActiveCategoriesAsync()
        {
            var categories = await _unitOfWork.MedicineCategories.FindAsync(c => c.IsActive);
            return _mapper.Map<IEnumerable<MedicineCategoryVM>>(categories);
        }
    }
}
