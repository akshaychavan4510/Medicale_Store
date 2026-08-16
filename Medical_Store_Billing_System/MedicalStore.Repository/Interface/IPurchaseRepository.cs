using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface IPurchaseRepository : IGenericRepository<PurchaseMaster>
    {
        Task<PurchaseMaster?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<PurchaseMaster>> GetBySupplierIdAsync(int supplierId);        // ← matches PurchaseService
        Task<IEnumerable<PurchaseMaster>> GetByDateRangeAsync(DateTime from, DateTime to);  // ← matches PurchaseService
        Task<decimal> GetTotalAmountAsync(DateTime from, DateTime to);                  // ← matches PurchaseService
        Task<IEnumerable<PurchaseDetails>> GetPurchaseDetailsByMedicineAsync(int medicineId);
        Task<PurchaseMaster> CreatePurchaseWithDetailsAsync(PurchaseMaster purchaseMaster, IEnumerable<PurchaseDetails> purchaseDetails);
        Task<IEnumerable<PurchaseMaster>> GetAllWithSupplierAsync();
        IQueryable<PurchaseMaster> GetQueryable();
    }
}
