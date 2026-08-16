// Repository/Implementations/PurchaseRepository.cs
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class PurchaseRepository : GenericRepository<PurchaseMaster>, IPurchaseRepository
    {
        public PurchaseRepository(ApplicationDbContext context) : base(context) { }

        public async Task<PurchaseMaster?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseDetails).ThenInclude(pd => pd.Medicine)
                .FirstOrDefaultAsync(p => p.PurchaseId == id);
        }

        public async Task<IEnumerable<PurchaseMaster>> GetBySupplierIdAsync(int supplierId)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseDetails).ThenInclude(pd => pd.Medicine)
                .Where(p => p.SuppId == supplierId)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<PurchaseMaster>> GetByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseDetails).ThenInclude(pd => pd.Medicine)
                .Where(p => p.PurchaseDate.Date >= from.Date && p.PurchaseDate.Date <= to.Date)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalAmountAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(p => p.PurchaseDate.Date >= from.Date && p.PurchaseDate.Date <= to.Date)
                .SumAsync(p => p.GrandTotal);
        }

        public async Task<IEnumerable<PurchaseDetails>> GetPurchaseDetailsByMedicineAsync(int medicineId)
        {
            return await _context.Set<PurchaseDetails>()
                .Where(pd => pd.MedId == medicineId)
                .ToListAsync();
        }

        public async Task<PurchaseMaster> CreatePurchaseWithDetailsAsync(
            PurchaseMaster purchaseMaster, IEnumerable<PurchaseDetails> purchaseDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Set<PurchaseMaster>().AddAsync(purchaseMaster);
                await _context.SaveChangesAsync();

                var detailsList = purchaseDetails.ToList();
                foreach (var detail in detailsList)
                {
                    detail.PurchaseId = purchaseMaster.PurchaseId;
                    var medicine = await _context.Set<MedicineMaster>().FindAsync(detail.MedId)
                        ?? throw new KeyNotFoundException($"Medicine ID {detail.MedId} not found.");
                    medicine.Stock += detail.Qty;
                    _context.Set<MedicineMaster>().Update(medicine);
                }

                await _context.Set<PurchaseDetails>().AddRangeAsync(detailsList);

                var supplier = await _context.Set<Supplier>().FindAsync(purchaseMaster.SuppId)
                    ?? throw new KeyNotFoundException($"Supplier ID {purchaseMaster.SuppId} not found.");
                supplier.SuppBal += purchaseMaster.GrandTotal;   // ← SuppBal
                _context.Set<Supplier>().Update(supplier);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return purchaseMaster;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public IQueryable<PurchaseMaster> GetQueryable()
        {
            return _dbSet
                .Include(p => p.Supplier)
                .Include(p => p.PurchaseDetails).ThenInclude(pd => pd.Medicine)
                .AsQueryable();
        }
        public async Task<IEnumerable<PurchaseMaster>> GetAllWithSupplierAsync()
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();
        }

    }
}