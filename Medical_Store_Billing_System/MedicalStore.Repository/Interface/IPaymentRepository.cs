using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetPaymentsBySupplierAsync(int supplierId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalPaymentsAsync(DateTime from, DateTime to);
    }
}
