// MedicalStore.Business/Interfaces/IReceiptService.cs
using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IReceiptService
    {
        Task<IEnumerable<ReceiptVM>> GetAllReceiptsAsync();
        Task<ReceiptVM?> GetReceiptByIdAsync(int receiptId);
        Task<ReceiptVM> CreateReceiptAsync(ReceiptVM model);
        Task<ReceiptVM> UpdateReceiptAsync(ReceiptVM model);        // ← added for Edit
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<ReceiptVM>> GetReceiptsByCustomerAsync(int customerId);
        Task<IEnumerable<ReceiptVM>> GetReceiptsByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalReceiptsAsync(DateTime from, DateTime to);
    }
}
