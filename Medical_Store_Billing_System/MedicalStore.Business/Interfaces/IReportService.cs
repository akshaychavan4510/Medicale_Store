using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IReportService
    {
        Task<IEnumerable<DailySaleReportVM>> GetDailySaleReportAsync(DateTime from, DateTime to);
        Task<IEnumerable<PurchaseReportVM>> GetPurchaseReportAsync(DateTime from, DateTime to);
        Task<CustomerLedgerVM> GetCustomerLedgerAsync(int customerId, DateTime from, DateTime to);
        Task<SupplierLedgerVM> GetSupplierLedgerAsync(int supplierId, DateTime from, DateTime to);
        Task<IEnumerable<StockReportVM>> GetStockReportAsync();
    }
}
