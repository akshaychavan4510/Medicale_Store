using Medical_Store_Billing_System.Models;
using MedicalStore.Data;

using MedicalStore.MedicalStore.Repository.Interface;

using Microsoft.EntityFrameworkCore;

namespace MedicalStore.Repository.Implementations
{
    public class MedicineMasterRepository : GenericRepository<MedicineMaster>, IMedicineMasterRepository
    {
        public MedicineMasterRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<MedicineMaster>> GetAllWithCategoryAndBrandAsync()
        {
            return await _dbSet
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .OrderBy(m => m.MedName)
                .ToListAsync();
        }

        public async Task<MedicineMaster?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .FirstOrDefaultAsync(m => m.MedId == id);
        }

        public async Task<IEnumerable<MedicineMaster>> GetLowStockMedicinesAsync(int threshold = 50)
        {
            return await _dbSet
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .Where(m => m.Stock <= threshold && m.IsActive)
                .OrderBy(m => m.Stock)
                .ToListAsync();
        }

        public async Task<bool> ExistsByNameAsync(string medicineName, int? excludeId = null)
        {
            var query = _dbSet.Where(m => m.MedName.ToLower() == medicineName.ToLower());

            if (excludeId.HasValue)
                query = query.Where(m => m.MedId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task UpdateStockAsync(int medicineId, decimal quantity, bool isIncrease)
        {
            var medicine = await _dbSet.FindAsync(medicineId)
                ?? throw new KeyNotFoundException($"Medicine with ID {medicineId} not found.");

            if (isIncrease)
                medicine.Stock += quantity;
            else
            {
                if (medicine.Stock < quantity)
                    throw new InvalidOperationException($"Insufficient stock for medicine '{medicine.MedName}'. Available: {medicine.Stock}, Requested: {quantity}.");
                medicine.Stock -= quantity;
            }

            _dbSet.Update(medicine);
        }

        public async Task<IEnumerable<MedicineMaster>> GetActiveMedicinesAsync()
        {
            return await _dbSet
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .Where(m => m.IsActive)
                .OrderBy(m => m.MedName)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicineMaster>> GetMedicinesByCategoryAsync(int categoryId)
        {
            return await _dbSet
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .Where(m => m.CatId == categoryId && m.IsActive)
                .OrderBy(m => m.MedName)
                .ToListAsync();
        }

        public async Task<IEnumerable<MedicineMaster>> GetMedicinesByBrandAsync(int brandId)
        {
            return await _dbSet
                .Include(m => m.Category)
                .Include(m => m.Brand)
                .Where(m => m.BrandId == brandId && m.IsActive)
                .OrderBy(m => m.MedName)
                .ToListAsync();
        }
    }
}