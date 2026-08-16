using Medical_Store_Billing_System.Models;
using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class MedicineCategoryRepository : GenericRepository<MedicineCategory>, IMedicineCategoryRepository
    {
        public MedicineCategoryRepository(ApplicationDbContext context) : base(context) { }

        public async Task<bool> ExistsByNameAsync(string categoryName, int? excludeId = null)
        {
            var query = _dbSet.Where(c => c.CatName.ToLower() == categoryName.ToLower());

            if (excludeId.HasValue)
                query = query.Where(c => c.CatId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<IEnumerable<MedicineCategory>> GetActiveCategoriesAsync()
        {
            return await _dbSet
                .Where(c => c.IsActive)
                .OrderBy(c => c.CatName)
                .ToListAsync();
        }
    }
}