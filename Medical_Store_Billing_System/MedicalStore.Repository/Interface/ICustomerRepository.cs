using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.Repository.Interfaces
{
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        Task<bool> ExistsByEmailAsync(string email, int? excludeId = null);
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();
        Task<Customer?> GetByIdWithSalesAsync(int id);
        Task UpdateBalanceAsync(int customerId, decimal amount, bool isIncrease);
    }
}