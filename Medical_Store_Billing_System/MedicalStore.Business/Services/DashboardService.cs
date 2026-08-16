using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;

using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class DashboardService : IDashboardService
    {
        private const int LowStockThreshold = 10;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<DashboardVM> GetDashboardDataAsync()
        {
            var today = DateTime.Today;
            var monthStart = new DateTime(today.Year, today.Month, 1);
            var trendStart = today.AddDays(-6);

            var sales = _unitOfWork.Sales.GetQueryable();
            var purchases = _unitOfWork.Purchases.GetQueryable();
            var medicines = _unitOfWork.Medicines.GetQueryable();
            var customers = _unitOfWork.Customers.GetQueryable();
            var suppliers = _unitOfWork.Suppliers.GetQueryable();

            var dashboard = new DashboardVM
            {
                TodaySales = await sales
                    .Where(s => s.SaleDate.Date == today)
                    .SumAsync(s => (decimal?)s.GrandTotal) ?? 0,

                MonthSales = await sales
                    .Where(s => s.SaleDate >= monthStart)
                    .SumAsync(s => (decimal?)s.GrandTotal) ?? 0,

                TodayPurchases = await purchases
                    .Where(p => p.PurchaseDate.Date == today)
                    .SumAsync(p => (decimal?)p.GrandTotal) ?? 0,

                TotalCustomers = await customers.CountAsync(),
                TotalSuppliers = await suppliers.CountAsync(),
                TotalMedicines = await medicines.CountAsync(),
                LowStockCount = await medicines.CountAsync(m => m.Stock <= LowStockThreshold),

                TotalReceivable = await customers.SumAsync(c => (decimal?)c.CustBal) ?? 0,
                TotalPayable = await suppliers.SumAsync(s => (decimal?)s.SuppBal) ?? 0
            };

            dashboard.SalesTrend = await sales
                .Where(s => s.SaleDate >= trendStart)
                .GroupBy(s => s.SaleDate.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DashboardChartPointVM
                {
                    Label = g.Key.ToString("MMM dd"),
                    Value = g.Sum(x => x.GrandTotal)
                })
                .ToListAsync();

            dashboard.LowStockItems = await medicines
                .Where(m => m.Stock <= LowStockThreshold)
                .OrderBy(m => m.Stock)
                .Select(m => new LowStockItemVM
                {
                    MedicineId = m.MedId,
                    MedicineName = m.MedName,
                    Stock = m.Stock
                })
                .Take(10)
                .ToListAsync();

            _logger.LogInformation("Dashboard data generated at {Time}.", DateTime.Now);
            return dashboard;
        }
    }
}
