using MedicalStore.Data;
using MedicalStore.MedicalStore.Repository.Implementations;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace MedicalStore.Repository.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _currentTransaction;

        private IMedicineCategoryRepository? _medicineCategories;
        private IBrandRepository? _brands;
        private IMedicineMasterRepository? _medicines;
        private ICustomerRepository? _customers;
        private ISupplierRepository? _suppliers;
        private ISaleRepository? _sales;
        private IPurchaseRepository? _purchases;
        private IReceiptRepository? _receipts;
        private IPaymentRepository? _payments;
        private IPurchaseDetailsRepository? _purchaseDetails;
        private ISaleDetailsRepository? _saleDetails;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IMedicineCategoryRepository MedicineCategories
        {
            get
            {
                if (_medicineCategories == null)
                    _medicineCategories = new MedicineCategoryRepository(_context);
                return _medicineCategories;
            }
        }

        public IBrandRepository Brands
        {
            get
            {
                if (_brands == null)
                    _brands = new BrandRepository(_context);
                return _brands;
            }
        }

        public IMedicineMasterRepository Medicines
        {
            get
            {
                if (_medicines == null)
                    _medicines = new MedicineMasterRepository(_context);
                return _medicines;
            }
        }

        public ICustomerRepository Customers
        {
            get
            {
                if (_customers == null)
                    _customers = new CustomerRepository(_context);
                return _customers;
            }
        }

        public ISupplierRepository Suppliers
        {
            get
            {
                if (_suppliers == null)
                    _suppliers = new SupplierRepository(_context);
                return _suppliers;
            }
        }

        public ISaleRepository Sales
        {
            get
            {
                if (_sales == null)
                    _sales = new SaleRepository(_context);
                return _sales;
            }
        }

        public IPurchaseRepository Purchases
        {
            get
            {
                if (_purchases == null)
                    _purchases = new PurchaseRepository(_context);
                return _purchases;
            }
        }

        public IReceiptRepository Receipts
        {
            get
            {
                if (_receipts == null)
                    _receipts = new ReceiptRepository(_context);
                return _receipts;
            }
        }

        public IPaymentRepository Payments
        {
            get
            {
                if (_payments == null)
                    _payments = new PaymentRepository(_context);
                return _payments;
            }
        }

        public IPurchaseDetailsRepository PurchaseDetails
        {
            get
            {
                if (_purchaseDetails == null)
                    _purchaseDetails = new PurchaseDetailsRepository(_context);
                return _purchaseDetails;
            }
        }

        public ISaleDetailsRepository SaleDetails
        {
            get
            {
                if (_saleDetails == null)
                    _saleDetails = new SaleDetailsRepository(_context);
                return _saleDetails;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            _currentTransaction = await _context.Database.BeginTransactionAsync();
            return _currentTransaction;
        }

        public async Task CommitAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync();
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
        }
    }

}