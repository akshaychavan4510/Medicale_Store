using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class SupplierRepository : GenericRepository<Supplier>, ISupplierRepository
    {
        public SupplierRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(s => s.SuppEmail != null && s.SuppEmail.ToLower() == email.ToLower());
            if (excludeId.HasValue)
                query = query.Where(s => s.SuppId != excludeId.Value);
            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Supplier>> GetActiveSuppliersAsync()
        {
            return await _dbSet
                .Where(s => s.IsActive)
                .OrderBy(s => s.SuppName)
                .ToListAsync();
        }

        public async Task<Supplier?> GetByIdWithPurchasesAsync(int id)
        {
            return await _dbSet
                .Include(s => s.PurchaseMasters)
                    .ThenInclude(p => p.PurchaseDetails)
                        .ThenInclude(pd => pd.Medicine)
                .Include(s => s.Payments)
                .FirstOrDefaultAsync(s => s.SuppId == id);
        }

        public async Task UpdateBalanceAsync(int supplierId, decimal amount, bool isIncrease)
        {
            var supplier = await _dbSet.FindAsync(supplierId)
                ?? throw new KeyNotFoundException($"Supplier with ID {supplierId} not found.");
            if (isIncrease)
                supplier.SuppBal += amount;
            else
            {
                if (supplier.SuppBal < amount)
                    throw new InvalidOperationException($"Supplier balance ({supplier.SuppBal}) is less than payment amount ({amount}).");
                supplier.SuppBal -= amount;
            }
            _dbSet.Update(supplier);
        }
    }
}
