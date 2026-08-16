using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface ISaleService
    {
        Task<IEnumerable<SaleMasterVM>> GetAllAsync();
        Task<SaleMasterVM?> GetByIdAsync(int id);
        Task<bool> CreateSaleAsync(SaleMasterVM model);
        Task<bool> DeleteAsync(int id);
    }
}
