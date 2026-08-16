using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IPurchaseService
    {
        Task<IEnumerable<PurchaseMasterVM>> GetAllAsync();
        Task<PurchaseMasterVM?> GetByIdAsync(int id);
        Task<bool> CreatePurchaseAsync(PurchaseMasterVM model);
        Task<bool> UpdatePurchaseAsync(PurchaseMasterVM model);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<PurchaseMasterVM>> GetPurchasesBySupplierAsync(int supplierId);
        Task<IEnumerable<PurchaseMasterVM>> GetPurchasesByDateRangeAsync(DateTime from, DateTime to);
        Task<decimal> GetTotalPurchaseAmountAsync(DateTime from, DateTime to);

        /// <summary>
        /// Returns next sequential PurId (max+1) and auto-generated Invoice No (INV-0001).
        /// </summary>
        Task<(int NextPurId, string NextInvoiceNo)> GetNextPurchaseNumberAsync();
    }
}