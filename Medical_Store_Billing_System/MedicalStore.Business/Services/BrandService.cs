using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using MedicalStore.Repository.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandService> _logger;

        public BrandService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<BrandService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<BrandVM>> GetAllAsync()
        {
            var brands = await _unitOfWork.Brands.GetAllAsync();
            return _mapper.Map<IEnumerable<BrandVM>>(brands);
        }

        public async Task<BrandVM?> GetByIdAsync(int id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            return brand == null ? null : _mapper.Map<BrandVM>(brand);
        }

        // ─── Get Brand Details with additional info ───
        public async Task<BrandDetailsVM> GetBrandDetailsAsync(int id)
        {
            var brand = await _unitOfWork.Brands.GetByIdAsync(id);
            if (brand == null)
                return null;

            var details = _mapper.Map<BrandDetailsVM>(brand);

            // Get related medicines count
            var medicines = await _unitOfWork.Medicines.FindAsync(m => m.BrandId == id);
            details.MedicineCount = medicines.Count();

            // Get total stock value for this brand
            // Use SaleRate (or PurchaseRate) - check your MedicineMaster entity for the correct property name
            details.TotalStockValue = medicines.Sum(m => m.Stock * m.SaleRate);

            return details;
        }

        public async Task<int> CreateAsync(BrandVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            // Check for duplicate brand name
            if (await IsBrandNameDuplicateAsync(model.BrandName))
                throw new InvalidOperationException($"Brand '{model.BrandName}' already exists.");

            var entity = _mapper.Map<Brand>(model);
            entity.CreatedDate = DateTime.UtcNow;
            entity.IsActive = true;

            await _unitOfWork.Brands.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Brand created: {BrandName} (ID: {BrandId})", entity.BrandName, entity.BrandId);
            return entity.BrandId;
        }

        public async Task<bool> UpdateAsync(BrandVM model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));

            var entity = await _unitOfWork.Brands.GetByIdAsync(model.BrandId)
                ?? throw new KeyNotFoundException($"Brand with ID {model.BrandId} not found.");

            // Check for duplicate brand name (excluding current brand)
            if (await IsBrandNameDuplicateAsync(model.BrandName, model.BrandId))
                throw new InvalidOperationException($"Brand '{model.BrandName}' already exists.");

            _mapper.Map(model, entity);
            entity.ModifiedDate = DateTime.UtcNow;

            _unitOfWork.Brands.Update(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Brand updated: {BrandName} (ID: {BrandId})", entity.BrandName, model.BrandId);
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _unitOfWork.Brands.GetByIdAsync(id);
            if (entity == null) return false;

            // Check if brand is used in any medicine
            var medicines = await _unitOfWork.Medicines.FindAsync(m => m.BrandId == id);
            if (medicines.Any())
                throw new InvalidOperationException($"Cannot delete brand '{entity.BrandName}' as it is associated with {medicines.Count()} medicine(s).");

            _unitOfWork.Brands.Delete(entity);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Brand deleted: {BrandName} (ID: {BrandId})", entity.BrandName, id);
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _unitOfWork.Brands.GetByIdAsync(id) != null;
        }

        public async Task<bool> IsBrandNameDuplicateAsync(string brandName, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(brandName)) return false;

            var allBrands = await _unitOfWork.Brands.FindAsync(
                b => b.BrandName.ToLower() == brandName.ToLower());

            return excludeId.HasValue
                ? allBrands.Any(b => b.BrandId != excludeId.Value)
                : allBrands.Any();
        }

        public async Task<IEnumerable<BrandVM>> GetActiveBrandsAsync()
        {
            var brands = await _unitOfWork.Brands.GetActiveBrandsAsync();
            return _mapper.Map<IEnumerable<BrandVM>>(brands);
        }
    }
}