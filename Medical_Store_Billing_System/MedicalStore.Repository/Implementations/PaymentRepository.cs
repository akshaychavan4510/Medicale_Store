using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(ApplicationDbContext context) : base(context)
        {
        }

        // ✅ FIX: Override GetAllAsync to eagerly load Supplier so SupplierName is populated
        public new async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _dbSet
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsBySupplierAsync(int supplierId)
        {
            return await _dbSet
                .Where(p => p.SuppId == supplierId)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= from.Date && p.PaymentDate <= to.Date)
                .Include(p => p.Supplier)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalPaymentsAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(p => p.PaymentDate >= from.Date && p.PaymentDate <= to.Date)
                .SumAsync(p => p.Amount);
        }
    }
}