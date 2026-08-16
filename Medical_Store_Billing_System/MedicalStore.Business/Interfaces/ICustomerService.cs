using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerVM>> GetAllAsync();
        Task<CustomerVM?> GetByIdAsync(int id);
        Task<int> CreateAsync(CustomerVM model);
        Task<bool> UpdateAsync(CustomerVM model);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> IsEmailDuplicateAsync(string email, int? excludeId = null);
        Task<decimal> GetBalanceAsync(int customerId);
        Task<bool> UpdateBalanceAsync(int customerId, decimal amount, bool isAddition);
        Task<IEnumerable<CustomerVM>> GetCustomersWithOutstandingBalance();
        Task<IEnumerable<CustomerVM>> GetActiveCustomersAsync();
    }
}
