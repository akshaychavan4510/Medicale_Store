using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Implementations;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.MedicalStore.Repository.Implementations
{

    public class SaleDetailsRepository : GenericRepository<SaleDetails>, ISaleDetailsRepository
    {
        public SaleDetailsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<SaleDetails>> GetBySaleIdAsync(int saleId)
        {
            return await _dbSet
                .Where(sd => sd.SaleId == saleId)
                .Include(sd => sd.Medicine)
                .ToListAsync();
        }

        public async Task<IEnumerable<SaleDetails>> GetByMedicineIdAsync(int medicineId)
        {
            return await _dbSet
                .Where(sd => sd.MedId == medicineId)
                .Include(sd => sd.SaleMaster)
                .OrderByDescending(sd => sd.SaleMaster.SaleDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSaleAmountByMedicineAsync(int medicineId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(sd => sd.MedId == medicineId);

            if (fromDate.HasValue)
                query = query.Where(sd => sd.SaleMaster.SaleDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(sd => sd.SaleMaster.SaleDate <= toDate.Value);

            return await query.SumAsync(sd => sd.Total);
        }
    }
}
