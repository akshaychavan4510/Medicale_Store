using MedicalStore.Business.ViewModels;

namespace MedicalStore.MedicalStore.Business.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardVM> GetDashboardDataAsync();  // ← was GetDashboardAsync(), now matches implementation
    }
}
