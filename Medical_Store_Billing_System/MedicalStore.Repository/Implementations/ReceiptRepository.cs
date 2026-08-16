// MedicalStore.Repository/Implementations/ReceiptRepository.cs
using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class ReceiptRepository : GenericRepository<Receipt>, IReceiptRepository
    {
        public ReceiptRepository(ApplicationDbContext context) : base(context) { }

        // ← NEW: eager-loads Customer so AutoMapper can map CustomerName
        public async Task<IEnumerable<Receipt>> GetAllWithCustomerAsync()
        {
            return await _dbSet
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }

        // ← NEW: eager-loads Customer for Details / Edit / Delete views
        public async Task<Receipt?> GetByIdWithCustomerAsync(int receiptId)
        {
            return await _dbSet
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.ReceiptId == receiptId);
        }

        public async Task<IEnumerable<Receipt>> GetReceiptsByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Where(r => r.CustId == customerId)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Receipt>> GetReceiptsByDateRangeAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(r => r.ReceiptDate >= from.Date && r.ReceiptDate <= to.Date)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.ReceiptDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalReceiptsAsync(DateTime from, DateTime to)
        {
            return await _dbSet
                .Where(r => r.ReceiptDate >= from.Date && r.ReceiptDate <= to.Date)
                .SumAsync(r => r.Amount);
        }
    }
}
