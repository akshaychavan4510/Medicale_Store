using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IMedicineMasterService
    {
        Task<IEnumerable<MedicineMasterVM>> GetAllAsync();
        Task<MedicineMasterVM?> GetByIdAsync(int id);
        Task<int> CreateAsync(MedicineMasterVM model);
        Task<bool> UpdateAsync(MedicineMasterVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsMedicineNameDuplicateAsync(string medicineName, int? excludeId = null);
        Task<IEnumerable<MedicineMasterVM>> GetLowStockMedicinesAsync(int threshold = 50);
        Task<IEnumerable<MedicineMasterVM>> GetByCategoryAsync(int categoryId);
        Task<IEnumerable<MedicineMasterVM>> GetByBrandAsync(int brandId);
        Task<decimal> GetCurrentStockAsync(int medicineId);
        Task<bool> UpdateStockAsync(int medicineId, decimal quantity, bool isAddition);
        Task<IEnumerable<MedicineMasterVM>> GetActiveMedicinesAsync();
        Task<IEnumerable<MedicineMasterVM>> SearchMedicinesAsync(string searchTerm);
        Task<IEnumerable<MedicineMasterVM>> GetExpiringMedicinesAsync(int daysThreshold = 30);
    }
}
