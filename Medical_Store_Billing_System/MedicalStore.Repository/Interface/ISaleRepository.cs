using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Repository.Interface
{
    public interface ISaleRepository : IGenericRepository<SaleMaster>
    {
        Task<SaleMaster?> GetByIdWithDetailsAsync(int id);
        Task<IEnumerable<SaleMaster>> GetSalesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleMaster>> GetSalesByCustomerAsync(int customerId);
        Task<decimal> GetTotalSaleAmountAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<SaleDetails>> GetSaleDetailsByMedicineAsync(int medicineId);
        Task<IEnumerable<SaleMaster>> GetAllWithDetailsAsync();

        Task<SaleMaster> CreateSaleWithDetailsAsync(SaleMaster saleMaster, IEnumerable<SaleDetails> saleDetails);
    }
}
