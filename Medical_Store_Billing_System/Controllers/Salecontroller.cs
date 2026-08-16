using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class SaleController : Controller
    {
        private readonly ISaleService _saleService;
        private readonly ICustomerService _customerService;
        private readonly IMedicineMasterService _medicineService;

        public SaleController(
            ISaleService saleService,
            ICustomerService customerService,
            IMedicineMasterService medicineService)
        {
            _saleService = saleService ?? throw new ArgumentNullException(nameof(saleService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _medicineService = medicineService ?? throw new ArgumentNullException(nameof(medicineService));
        }

        // GET: /Sale
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL sales from database
            var allSales = await _saleService.GetAllAsync();

            // Store all sales for JavaScript search (client-side search across all pages)
            ViewBag.AllSales = allSales;

            // Apply server-side search filter
            IEnumerable<SaleMasterVM> filteredSales = allSales;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredSales = filteredSales.Where(x =>
                    x.SaleId.ToString().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(x.CustomerName) &&
                     x.CustomerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.SaleDate.ToString("dd-MM-yyyy").Contains(searchTerm) ||
                    x.SaleDetails.Any(d =>
                        (!string.IsNullOrEmpty(d.MedicineName) &&
                         d.MedicineName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                    )
                );
            }

            // Get total records for pagination
            int totalRecords = filteredSales.Count();

            // Apply pagination
            var paginatedData = filteredSales
                .OrderByDescending(s => s.SaleId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Set ViewBag properties
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.PageSize = pageSize;

            // Calculate stats for display
            ViewBag.TotalAmount = allSales.Sum(s => s.GrandTotal);

            return View(paginatedData);
        }

        // GET: /Sale/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new SaleMasterVM { SaleDate = DateTime.Now });
        }

        // POST: /Sale/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaleMasterVM vm)
        {
            // 1. Must have at least one detail line
            if (vm.SaleDetails == null || !vm.SaleDetails.Any())
            {
                ModelState.AddModelError("", "Please add at least one medicine line.");
                await PopulateDropdownsAsync();
                return View(vm);
            }

            // 2. Strip server-side validation for fields that are COMPUTED or display-only.
            var keysToRemove = ModelState.Keys
                .Where(k =>
                    k == "GrandTotal" ||
                    k == "CustomerName" ||
                    k.EndsWith(".Amt") ||
                    k.EndsWith(".GstAmt") ||
                    k.EndsWith(".Gst") ||
                    k.EndsWith(".Total") ||
                    k.EndsWith(".MedicineName"))
                .ToList();

            foreach (var key in keysToRemove)
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            try
            {
                if (await _saleService.CreateSaleAsync(vm))
                {
                    TempData["Success"] = "Sale created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError("", "Sale could not be saved. Please try again.");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An unexpected error occurred: " + ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(vm);
        }

        // GET: /Sale/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _saleService.GetByIdAsync(id);
            return vm == null ? NotFound() : View(vm);
        }

        // POST: /Sale/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (await _saleService.DeleteAsync(id))
                TempData["Success"] = "Sale deleted successfully.";
            else
                TempData["Error"] = "Cannot delete this sale.";

            return RedirectToAction(nameof(Index));
        }

        // GET: /Sale/Invoice/5
        public async Task<IActionResult> Invoice(int id)
        {
            var sale = await _saleService.GetByIdAsync(id);
            return sale == null ? NotFound() : View(sale);
        }

        // AJAX: GET /Sale/GetMedicineInfo?id=3
        [HttpGet]
        public async Task<IActionResult> GetMedicineInfo(int id)
        {
            var med = await _medicineService.GetByIdAsync(id);
            if (med == null) return NotFound();

            return Json(new
            {
                rate = med.SaleRate,
                stock = med.Stock,
                name = med.MedName
            });
        }

        // ── private helpers ───────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync()
        {
            var customers = await _customerService.GetAllAsync();
            var medicines = await _medicineService.GetAllAsync();

            ViewBag.Customers = new SelectList(customers, "CustId", "CustName");
            ViewBag.Medicines = new SelectList(medicines, "MedId", "MedName");
        }
    }
}