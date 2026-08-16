using MedicalStore.Business.ViewModels;

namespace MedicalStore.Business.Interfaces
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandVM>> GetAllAsync();
        Task<BrandVM?> GetByIdAsync(int id);
        Task<BrandDetailsVM> GetBrandDetailsAsync(int id); // NEW
        Task<int> CreateAsync(BrandVM model);
        Task<bool> UpdateAsync(BrandVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsBrandNameDuplicateAsync(string brandName, int? excludeId = null);
        Task<IEnumerable<BrandVM>> GetActiveBrandsAsync();
    }
}