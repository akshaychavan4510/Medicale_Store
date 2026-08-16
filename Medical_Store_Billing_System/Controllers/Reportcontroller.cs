// Controllers/ReportController.cs
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ICustomerService _customerService;
        private readonly ISupplierService _supplierService;

        public ReportController(
            IReportService reportService,
            ICustomerService customerService,
            ISupplierService supplierService)
        {
            _reportService = reportService ?? throw new ArgumentNullException(nameof(reportService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
        }

        // GET: /Report/Index
        public IActionResult Index() => View();

        // GET: /Report/DailySale
        public async Task<IActionResult> DailySale(DateTime? from, DateTime? to)
        {
            var f = from ?? DateTime.Today;
            var t = to ?? DateTime.Today;
            ViewBag.From = f.ToString("yyyy-MM-dd");
            ViewBag.To = t.ToString("yyyy-MM-dd");
            return View(await _reportService.GetDailySaleReportAsync(f, t));
        }

        // GET: /Report/PurchaseReport
        public async Task<IActionResult> PurchaseReport(DateTime? from, DateTime? to)
        {
            var f = from ?? DateTime.Today;
            var t = to ?? DateTime.Today;
            ViewBag.From = f.ToString("yyyy-MM-dd");
            ViewBag.To = t.ToString("yyyy-MM-dd");
            return View(await _reportService.GetPurchaseReportAsync(f, t));
        }

        // GET: /Report/CustomerLedger
        public async Task<IActionResult> CustomerLedger(int? custId, DateTime? from, DateTime? to)
        {
            await PopulateCustomersAsync(custId);
            ViewBag.CustId = custId;

            if (custId == null)
                return View(new CustomerLedgerVM());

            var f = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var t = to ?? DateTime.Today;
            ViewBag.From = f.ToString("yyyy-MM-dd");
            ViewBag.To = t.ToString("yyyy-MM-dd");

            return View(await _reportService.GetCustomerLedgerAsync(custId.Value, f, t));
        }

        // GET: /Report/SupplierLedger
        public async Task<IActionResult> SupplierLedger(int? supId, DateTime? from, DateTime? to)
        {
            await PopulateSuppliersAsync(supId);
            ViewBag.SupId = supId;

            if (supId == null)
                return View(new SupplierLedgerVM());

            var f = from ?? new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var t = to ?? DateTime.Today;
            ViewBag.From = f.ToString("yyyy-MM-dd");
            ViewBag.To = t.ToString("yyyy-MM-dd");

            return View(await _reportService.GetSupplierLedgerAsync(supId.Value, f, t));
        }

        // GET: /Report/StockReport
        public async Task<IActionResult> StockReport(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL stock items from database
            var allStockItems = await _reportService.GetStockReportAsync();

            // Store all stock items for JavaScript search (client-side search across all pages)
            ViewBag.AllStockItems = allStockItems;

            // Apply server-side search filter
            IEnumerable<StockReportVM> filteredItems = allStockItems;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredItems = filteredItems.Where(x =>
                    (!string.IsNullOrEmpty(x.MedicineName) &&
                     x.MedicineName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.CategoryName) &&
                     x.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.BrandName) &&
                     x.BrandName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.Stock.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredItems.Count();

            // Apply pagination
            var paginatedData = filteredItems
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Set ViewBag properties
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.PageSize = pageSize;

            // Calculate summary stats
            ViewBag.NormalStock = allStockItems.Count(m => !m.IsLowStock && m.Stock > 0);
            ViewBag.LowStock = allStockItems.Count(m => m.IsLowStock && m.Stock > 0);
            ViewBag.OutOfStock = allStockItems.Count(m => m.Stock == 0);
            ViewBag.TotalStockValue = allStockItems.Sum(m => m.StockValue);

            return View(paginatedData);
        }

        private async Task PopulateCustomersAsync(int? selectedId = null)
        {
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "CustId", "CustName", selectedId);
        }

        private async Task PopulateSuppliersAsync(int? selectedId = null)
        {
            var suppliers = await _supplierService.GetAllAsync();
            ViewBag.Suppliers = new SelectList(suppliers, "SuppId", "SuppName", selectedId);
        }
    }
}