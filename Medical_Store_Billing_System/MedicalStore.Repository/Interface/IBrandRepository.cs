using Medical_Store_Billing_System.Models;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.MedicalStore.Repository.Interface.MedicalStore.Repository.Interfaces;

namespace MedicalStore.Repository.Interfaces
{
    public interface IBrandRepository : IGenericRepository<Brand>
    {
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
        Task<IEnumerable<Brand>> GetActiveBrandsAsync();
    }
}