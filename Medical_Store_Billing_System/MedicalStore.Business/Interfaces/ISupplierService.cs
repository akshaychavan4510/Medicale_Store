using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface ISupplierService
    {
        Task<IEnumerable<SupplierVM>> GetAllAsync();
        Task<SupplierVM?> GetByIdAsync(int id);
        Task<SupplierVM?> GetByIdWithPurchasesAsync(int id);
        Task<int> CreateAsync(SupplierVM model);
        Task<bool> UpdateAsync(SupplierVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsEmailDuplicateAsync(string email, int? excludeId = null);
        Task<decimal> GetBalanceAsync(int supplierId);
        Task<bool> UpdateBalanceAsync(int supplierId, decimal amount, bool isAddition);
        Task<IEnumerable<SupplierVM>> GetSuppliersWithOutstandingBalance();
        Task<IEnumerable<SupplierVM>> GetActiveSuppliersAsync();
    }
}
