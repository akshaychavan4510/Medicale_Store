using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface IMedicineCategoryRepository : IGenericRepository<MedicineCategory>
    {
        Task<bool> ExistsByNameAsync(string categoryName, int? excludeId = null);
        Task<IEnumerable<MedicineCategory>> GetActiveCategoriesAsync();
    }
}
