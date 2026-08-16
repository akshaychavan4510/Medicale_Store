using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface IMedicineMasterRepository : IGenericRepository<MedicineMaster>
    {
        Task<IEnumerable<MedicineMaster>> GetAllWithCategoryAndBrandAsync();
        Task<MedicineMaster?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<MedicineMaster>> GetLowStockMedicinesAsync(int threshold = 50);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task UpdateStockAsync(int medicineId, decimal quantity, bool isIncrease);
        Task<IEnumerable<MedicineMaster>> GetActiveMedicinesAsync();
        Task<IEnumerable<MedicineMaster>> GetMedicinesByCategoryAsync(int categoryId);
        Task<IEnumerable<MedicineMaster>> GetMedicinesByBrandAsync(int brandId);
    }
}
