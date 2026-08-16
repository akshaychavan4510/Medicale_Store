using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class PurchaseController : Controller
    {
        private readonly IPurchaseService _purchaseService;
        private readonly ISupplierService _supplierService;
        private readonly IMedicineMasterService _medicineService;

        public PurchaseController(
            IPurchaseService purchaseService,
            ISupplierService supplierService,
            IMedicineMasterService medicineService)
        {
            _purchaseService = purchaseService;
            _supplierService = supplierService;
            _medicineService = medicineService;
        }

        // GET: Purchase/Index
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL purchases from database
            var allPurchases = await _purchaseService.GetAllAsync();

            // Store all purchases for JavaScript search (client-side search across all pages)
            ViewBag.AllPurchases = allPurchases;

            // Apply server-side search filter
            IEnumerable<PurchaseMasterVM> filteredPurchases = allPurchases;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredPurchases = filteredPurchases.Where(x =>
                    x.PurId.ToString().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(x.InvoiceNo) &&
                     x.InvoiceNo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.SupplierName) &&
                     x.SupplierName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.PurchaseDate.ToString("dd-MM-yyyy").Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredPurchases.Count();

            // Apply pagination
            var paginatedData = filteredPurchases
                .OrderByDescending(p => p.PurId)
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
            ViewBag.TotalAmount = allPurchases.Sum(p => p.GrandTotal);

            return View(paginatedData);
        }

        // GET: Purchase/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();

            // Get next sequential ID and auto-generated invoice number
            var (nextPurId, nextInvoiceNo) = await _purchaseService.GetNextPurchaseNumberAsync();

            ViewBag.NextPurId = nextPurId;
            ViewBag.NextInvoiceNo = nextInvoiceNo;

            var vm = new PurchaseMasterVM
            {
                PurchaseDate = DateTime.Now,
                InvoiceNo = nextInvoiceNo   // pre-fills InvoiceNo in the VM too
            };

            return View(vm);
        }

        // POST: Purchase/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseMasterVM vm)
        {
            if (vm.PurchaseDetails == null || !vm.PurchaseDetails.Any())
            {
                ModelState.AddModelError("", "Please add at least one medicine line.");
                await PopulateDropdownsAsync();
                return View(vm);
            }

            ModelState.Remove("GrandTotal");
            ModelState.Remove("NetTotal");
            foreach (var key in ModelState.Keys.Where(k =>
                k.Contains("Amt") || k.Contains("Total") || k.Contains("Gst") ||
                k.Contains("MedicineName") || k.Contains("SupplierName")).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            try
            {
                if (await _purchaseService.CreatePurchaseAsync(vm))
                {
                    TempData["Success"] = $"Purchase PUR-{vm.PurId:D4} created successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(vm);
        }

        // GET: Purchase/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _purchaseService.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // ── EDIT ─────────────────────────────────────────────────────────────

        // GET: Purchase/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _purchaseService.GetByIdAsync(id);
            if (vm == null) return NotFound();
            await PopulateDropdownsAsync();
            return View(vm);
        }

        // POST: Purchase/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PurchaseMasterVM vm)
        {
            if (vm.PurchaseDetails == null || !vm.PurchaseDetails.Any())
            {
                ModelState.AddModelError("", "Please add at least one medicine line.");
                await PopulateDropdownsAsync();
                return View(vm);
            }

            // Remove server-calculated fields from validation
            ModelState.Remove("GrandTotal");
            ModelState.Remove("NetTotal");
            foreach (var key in ModelState.Keys.Where(k =>
                k.Contains("Amt") || k.Contains("Total") || k.Contains("Gst") ||
                k.Contains("MedicineName") || k.Contains("SupplierName")).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            try
            {
                if (await _purchaseService.UpdatePurchaseAsync(vm))
                {
                    TempData["Success"] = $"Purchase PUR-{vm.PurId:D4} updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] = "Purchase not found or could not be updated.";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(vm);
        }

        // ── DELETE ────────────────────────────────────────────────────────────

        // GET: Purchase/Delete/5
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _purchaseService.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: Purchase/Delete  (ActionName keeps URL as /Purchase/Delete)
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (await _purchaseService.DeleteAsync(id))
                    TempData["Success"] = $"Purchase PUR-{id:D4} deleted successfully.";
                else
                    TempData["Error"] = "Purchase not found or could not be deleted.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting purchase: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ── AJAX ──────────────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> GetMedicineInfo(int id)
        {
            var med = await _medicineService.GetByIdAsync(id);
            if (med == null) return NotFound();
            return Json(new { rate = med.SaleRate, stock = med.Stock, name = med.MedName });
        }

        // ── HELPERS ───────────────────────────────────────────────────────────

        private async Task PopulateDropdownsAsync()
        {
            var suppliers = await _supplierService.GetAllAsync();
            var medicines = await _medicineService.GetAllAsync();
            ViewBag.Suppliers = new SelectList(suppliers, "SuppId", "SuppName");
            ViewBag.Medicines = new SelectList(medicines, "MedId", "MedName");
        }
    }
}