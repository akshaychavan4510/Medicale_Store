using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{

    public interface IPurchaseDetailsRepository : IGenericRepository<PurchaseDetails>
    {
        Task<IEnumerable<PurchaseDetails>> GetByPurchaseIdAsync(int purchaseId);
        Task<IEnumerable<PurchaseDetails>> GetByMedicineIdAsync(int medicineId);
        Task<decimal> GetTotalPurchaseAmountByMedicineAsync(int medicineId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
