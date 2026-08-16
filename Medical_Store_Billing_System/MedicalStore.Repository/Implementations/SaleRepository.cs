using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class SaleRepository : GenericRepository<SaleMaster>, ISaleRepository
    {
        public SaleRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<SaleMaster>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Medicine)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<SaleMaster>> GetSalesByCustomerAsync(int customerId)
        {
            return await _dbSet
                .Include(s => s.SaleDetails).ThenInclude(sd => sd.Medicine)
                .Where(s => s.CustId == customerId)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }
        public async Task<SaleMaster?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Medicine)
                .FirstOrDefaultAsync(s => s.SaleId == id);
        }
        public async Task<IEnumerable<SaleMaster>> GetSalesByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails).ThenInclude(sd => sd.Medicine)
                .Where(s => s.SaleDate.Date >= fromDate.Date && s.SaleDate.Date <= toDate.Date)
                .OrderByDescending(s => s.SaleDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalSaleAmountAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.SaleDate.Date >= startDate.Date && s.SaleDate.Date <= endDate.Date)
                .SumAsync(s => s.GrandTotal);
        }

        public async Task<IEnumerable<SaleDetails>> GetSaleDetailsByMedicineAsync(int medicineId)
        {
            return await _context.Set<SaleDetails>()
                .Where(sd => sd.MedId == medicineId)
                .ToListAsync();
        }
        public async Task<IEnumerable<SaleMaster>> GetAllAsync()
        {
            return await _dbSet
                .Include(s => s.Customer)
                .Include(s => s.SaleDetails)
                    .ThenInclude(sd => sd.Medicine)
                .OrderByDescending(s => s.SaleId)
                .ToListAsync();
        }
        public async Task<SaleMaster> CreateSaleWithDetailsAsync(SaleMaster saleMaster, IEnumerable<SaleDetails> saleDetails)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.Set<SaleMaster>().AddAsync(saleMaster);
                await _context.SaveChangesAsync();

                var detailsList = saleDetails.ToList();
                foreach (var detail in detailsList)
                {
                    detail.SaleId = saleMaster.SaleId;
                    var medicine = await _context.Set<MedicineMaster>().FindAsync(detail.MedId)
                        ?? throw new KeyNotFoundException($"Medicine ID {detail.MedId} not found.");
                    if (medicine.Stock < detail.Qty)
                        throw new InvalidOperationException(
                            $"Insufficient stock for '{medicine.MedName}'. Available: {medicine.Stock}, Requested: {detail.Qty}.");
                    medicine.Stock -= detail.Qty;
                    _context.Set<MedicineMaster>().Update(medicine);
                }

                await _context.Set<SaleDetails>().AddRangeAsync(detailsList);

                var customer = await _context.Set<Customer>().FindAsync(saleMaster.CustId)
                    ?? throw new KeyNotFoundException($"Customer ID {saleMaster.CustId} not found.");
                customer.CustBal += saleMaster.GrandTotal;
                _context.Set<Customer>().Update(customer);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return saleMaster;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        
    }
}
