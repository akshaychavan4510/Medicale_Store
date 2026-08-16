using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface ISaleDetailsRepository : IGenericRepository<SaleDetails>
    {
        Task<IEnumerable<SaleDetails>> GetBySaleIdAsync(int saleId);
        Task<IEnumerable<SaleDetails>> GetByMedicineIdAsync(int medicineId);
        Task<decimal> GetTotalSaleAmountByMedicineAsync(int medicineId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
