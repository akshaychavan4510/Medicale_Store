using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsByEmailAsync(string email, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.CustEmail != null && c.CustEmail.ToLower() == email.ToLower());

            if (excludeId.HasValue)
                query = query.Where(c => c.CustId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Customer>> GetActiveCustomersAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.CustName)
                .ToListAsync();
        }

        public async Task<Customer?> GetByIdWithSalesAsync(int id)
        {
            return await _dbSet
                .Include(c => c.Sales)
                    .ThenInclude(s => s.SaleDetails)
                        .ThenInclude(sd => sd.Medicine)
                .Include(c => c.Receipts)
                .FirstOrDefaultAsync(c => c.CustId == id);
        }

        public async Task UpdateBalanceAsync(int customerId, decimal amount, bool isIncrease)
        {
            var customer = await _dbSet.FindAsync(customerId)
                ?? throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

            if (isIncrease)
                customer.CustBal += amount;
            else
            {
                if (customer.CustBal < amount)
                    throw new InvalidOperationException($"Customer balance ({customer.CustBal}) is less than amount ({amount}).");
                customer.CustBal -= amount;
            }

            _dbSet.Update(customer);
        }
    }
}