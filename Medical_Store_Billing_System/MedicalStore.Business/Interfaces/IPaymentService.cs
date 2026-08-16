using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IPaymentService
    {
        Task<IEnumerable<PaymentVM>> GetAllPaymentsAsync();
        Task<PaymentVM?> GetPaymentByIdAsync(int id);
        Task<bool> CreatePaymentAsync(PaymentVM model);
        Task<bool> UpdatePaymentAsync(PaymentVM model);   // ← NEW
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PaymentVM>> GetPaymentsBySupplierAsync(int supplierId);
        Task<IEnumerable<PaymentVM>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalPaymentsAsync(DateTime from, DateTime to);
    }
}