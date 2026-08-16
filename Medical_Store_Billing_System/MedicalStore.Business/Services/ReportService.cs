// Business/Services/ReportService.cs
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedicalStore.Business.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;   // ← must inject ILogger

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<DailySaleReportVM>> GetDailySaleReportAsync(
            DateTime fromDate, DateTime toDate)
        {
            var report = await _unitOfWork.Sales.GetQueryable()
                .Where(s => s.SaleDate.Date >= fromDate.Date && s.SaleDate.Date <= toDate.Date)
                .Select(s => new DailySaleReportVM
                {
                    Date = s.SaleDate,
                    SaleId = s.SaleId,
                    InvoiceNo = s.SaleId.ToString(),
                    CustomerName = s.Customer != null ? s.Customer.CustName : "",
                    ItemCount = s.SaleDetails.Count(),
                    GrandTotal = s.GrandTotal
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            _logger.LogInformation("Daily sale report: {From}–{To}, {Count} rows.",
                fromDate, toDate, report.Count);
            return report;
        }

        public async Task<IEnumerable<PurchaseReportVM>> GetPurchaseReportAsync(
            DateTime fromDate, DateTime toDate)
        {
            var report = await _unitOfWork.Purchases.GetQueryable()
                .Where(p => p.PurchaseDate.Date >= fromDate.Date && p.PurchaseDate.Date <= toDate.Date)
                .Select(p => new PurchaseReportVM
                {
                    Date = p.PurchaseDate,
                    PurId = p.PurchaseId,
                    InvoiceNo = p.PurchaseId.ToString(),
                    SupplierName = p.Supplier != null ? p.Supplier.SuppName : "",
                    ItemCount = p.PurchaseDetails.Count(),
                    GrandTotal = p.GrandTotal
                })
                .OrderBy(r => r.Date)
                .ToListAsync();

            _logger.LogInformation("Purchase report: {From}–{To}, {Count} rows.",
                fromDate, toDate, report.Count);
            return report;
        }

        public async Task<CustomerLedgerVM> GetCustomerLedgerAsync(
            int customerId, DateTime fromDate, DateTime toDate)
        {
            var customer = await _unitOfWork.Customers.GetByIdAsync(customerId)
                ?? throw new InvalidOperationException($"Customer id {customerId} not found.");

            var saleEntries = await _unitOfWork.Sales.GetQueryable()
                .Where(s => s.CustId == customerId
                         && s.SaleDate.Date >= fromDate.Date
                         && s.SaleDate.Date <= toDate.Date)
                .Select(s => new LedgerEntryVM
                {
                    Date = s.SaleDate,
                    Particulars = "Sale #" + s.SaleId,
                    Debit = s.GrandTotal,
                    Credit = 0
                })
                .ToListAsync();

            var receiptEntries = await _unitOfWork.Receipts.GetQueryable()
                .Where(r => r.CustId == customerId
                         && r.ReceiptDate.Date >= fromDate.Date
                         && r.ReceiptDate.Date <= toDate.Date)
                .Select(r => new LedgerEntryVM
                {
                    Date = r.ReceiptDate,
                    Particulars = "Receipt (" + r.PayMode + ")",
                    Debit = 0,
                    Credit = r.Amount
                })
                .ToListAsync();

            var entries = saleEntries.Concat(receiptEntries).OrderBy(e => e.Date).ToList();
            decimal running = 0;
            foreach (var e in entries) { running += e.Debit - e.Credit; e.Balance = running; }

            return new CustomerLedgerVM
            {
                CustomerId = customer.CustId,
                CustomerName = customer.CustName,
                Entries = entries,
                ClosingBalance = customer.CustBal
            };
        }

        public async Task<SupplierLedgerVM> GetSupplierLedgerAsync(
            int supplierId, DateTime fromDate, DateTime toDate)
        {
            var supplier = await _unitOfWork.Suppliers.GetByIdAsync(supplierId)
                ?? throw new InvalidOperationException($"Supplier id {supplierId} not found.");

            var purchaseEntries = await _unitOfWork.Purchases.GetQueryable()
                .Where(p => p.SuppId == supplierId
                         && p.PurchaseDate.Date >= fromDate.Date
                         && p.PurchaseDate.Date <= toDate.Date)
                .Select(p => new LedgerEntryVM
                {
                    Date = p.PurchaseDate,
                    Particulars = "Purchase #" + p.PurchaseId,
                    Debit = p.GrandTotal,
                    Credit = 0
                })
                .ToListAsync();

            var paymentEntries = await _unitOfWork.Payments.GetQueryable()
                .Where(p => p.SuppId == supplierId
                         && p.PaymentDate.Date >= fromDate.Date
                         && p.PaymentDate.Date <= toDate.Date)
                .Select(p => new LedgerEntryVM
                {
                    Date = p.PaymentDate,
                    Particulars = "Payment (" + p.PayMode + ")",
                    Debit = 0,
                    Credit = p.Amount
                })
                .ToListAsync();

            var entries = purchaseEntries.Concat(paymentEntries).OrderBy(e => e.Date).ToList();
            decimal running = 0;
            foreach (var e in entries) { running += e.Debit - e.Credit; e.Balance = running; }

            return new SupplierLedgerVM
            {
                SupplierId = supplier.SuppId,
                SupplierName = supplier.SuppName,
                Entries = entries,
                ClosingBalance = supplier.SuppBal
            };
        }

        public async Task<IEnumerable<StockReportVM>> GetStockReportAsync()
        {
            return await _unitOfWork.Medicines.GetQueryable()
                .Select(m => new StockReportVM
                {
                    MedicineId = m.MedId,
                    MedicineName = m.MedName,
                    CategoryName = m.Category != null ? m.Category.CatName : "",
                    BrandName = m.Brand != null ? m.Brand.BrandName : "",
                    Stock = m.Stock,
                    Rate = m.SaleRate,
                    StockValue = m.Stock * m.SaleRate,
                    IsLowStock = m.Stock <= 10
                })
                .OrderBy(r => r.MedicineName)
                .ToListAsync();
        }
    }
}