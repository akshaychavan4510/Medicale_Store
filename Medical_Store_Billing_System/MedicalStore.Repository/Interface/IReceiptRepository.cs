// MedicalStore.Repository/Interface/IReceiptRepository.cs
using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface IReceiptRepository : IGenericRepository<Receipt>
    {
        // ← NEW: loads Receipt + Customer navigation property
        Task<IEnumerable<Receipt>> GetAllWithCustomerAsync();
        Task<Receipt?> GetByIdWithCustomerAsync(int receiptId);

        Task<IEnumerable<Receipt>> GetReceiptsByCustomerAsync(int customerId);
        Task<IEnumerable<Receipt>> GetReceiptsByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalReceiptsAsync(DateTime from, DateTime to);
    }
}
