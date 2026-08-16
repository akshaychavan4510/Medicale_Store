using MedicalStore.MedicalStore.Repository.Interface;

using Microsoft.EntityFrameworkCore.Storage;

namespace MedicalStore.Repository.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IMedicineCategoryRepository MedicineCategories { get; }
        IBrandRepository Brands { get; }
        IMedicineMasterRepository Medicines { get; }
        ICustomerRepository Customers { get; }
        ISupplierRepository Suppliers { get; }
        ISaleRepository Sales { get; }
        IPurchaseRepository Purchases { get; }
        IReceiptRepository Receipts { get; }
        IPaymentRepository Payments { get; }
        IPurchaseDetailsRepository PurchaseDetails { get; }
        ISaleDetailsRepository SaleDetails { get; }

        Task<int> SaveChangesAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}