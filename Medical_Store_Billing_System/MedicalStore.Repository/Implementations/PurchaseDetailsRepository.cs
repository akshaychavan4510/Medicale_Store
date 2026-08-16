using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Implementations;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.MedicalStore.Repository.Implementations
{
    public class PurchaseDetailsRepository : GenericRepository<PurchaseDetails>, IPurchaseDetailsRepository
    {
        public PurchaseDetailsRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<PurchaseDetails>> GetByPurchaseIdAsync(int purchaseId)
        {
            return await _dbSet
                .Where(pd => pd.PurchaseId == purchaseId)
                .Include(pd => pd.Medicine)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseDetails>> GetByMedicineIdAsync(int medicineId)
        {
            return await _dbSet
                .Where(pd => pd.MedId == medicineId)
                .Include(pd => pd.PurchaseMaster)
                .OrderByDescending(pd => pd.PurchaseMaster.PurchaseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPurchaseAmountByMedicineAsync(int medicineId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(pd => pd.MedId == medicineId);

            if (fromDate.HasValue)
                query = query.Where(pd => pd.PurchaseMaster.PurchaseDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(pd => pd.PurchaseMaster.PurchaseDate <= toDate.Value);

            return await query.SumAsync(pd => pd.Total);
        }
    }
}
