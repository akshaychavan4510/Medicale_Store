using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface ISupplierRepository : IGenericRepository<Supplier>
    {
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
        Task<IEnumerable<Supplier>> GetActiveSuppliersAsync();
        Task<Supplier?> GetByIdWithPurchasesAsync(int id);
        Task UpdateBalanceAsync(int supplierId, decimal amount, bool isIncrease);
    }
}
