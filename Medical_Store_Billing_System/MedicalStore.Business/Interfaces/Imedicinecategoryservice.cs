using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IMedicineCategoryService
    {
        Task<IEnumerable<MedicineCategoryVM>> GetAllAsync();
        Task<MedicineCategoryVM?> GetByIdAsync(int id);
        Task<int> CreateAsync(MedicineCategoryVM model);
        Task<bool> UpdateAsync(MedicineCategoryVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsCategoryNameDuplicateAsync(string categoryName, int? excludeId = null);
        Task<IEnumerable<MedicineCategoryVM>> GetActiveCategoriesAsync();
    }
}
