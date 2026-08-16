using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class BrandRepository : GenericRepository<Brand>, IBrandRepository
    {
        public BrandRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsByNameAsync(string brandName, int? excludeId = null)
        {
            var query = _dbSet.Where(b => b.BrandName.ToLower() == brandName.ToLower());

            if (excludeId.HasValue)
                query = query.Where(b => b.BrandId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<Brand>> GetActiveBrandsAsync()
        {
            return await _dbSet
                .Where(b => b.IsActive)
                .OrderBy(b => b.BrandName)
                .ToListAsync();
        }
    }
}