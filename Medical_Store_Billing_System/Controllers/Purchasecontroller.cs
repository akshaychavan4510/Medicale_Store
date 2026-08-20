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

        // ================================================================
        // INDEX
        // GET: /Purchase
        // GET: /Purchase/Index
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchTerm = "",
            int page = 1)
        {
            const int pageSize = 10;

            if (page < 1)
            {
                page = 1;
            }

            try
            {
                // --------------------------------------------------------
                // Get ALL purchases
                // --------------------------------------------------------
                var allPurchases =
                    await _purchaseService.GetAllAsync()
                    ?? Enumerable.Empty<PurchaseMasterVM>();

                // Materialize once
                var purchaseList = allPurchases.ToList();

                // --------------------------------------------------------
                // Keep ALL purchases for client-side JavaScript search
                // --------------------------------------------------------
                ViewBag.AllPurchases = purchaseList;

                // --------------------------------------------------------
                // SEARCH
                // --------------------------------------------------------
                var filteredPurchases = purchaseList.AsEnumerable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    searchTerm = searchTerm.Trim();

                    filteredPurchases = filteredPurchases.Where(p =>
                        p.PurId
                            .ToString()
                            .Contains(searchTerm, StringComparison.OrdinalIgnoreCase)

                        ||

                        (!string.IsNullOrWhiteSpace(p.InvoiceNo) &&
                         p.InvoiceNo.Contains(
                             searchTerm,
                             StringComparison.OrdinalIgnoreCase))

                        ||

                        (!string.IsNullOrWhiteSpace(p.SupplierName) &&
                         p.SupplierName.Contains(
                             searchTerm,
                             StringComparison.OrdinalIgnoreCase))

                        ||

                        p.PurchaseDate
                            .ToString("dd-MM-yyyy")
                            .Contains(
                                searchTerm,
                                StringComparison.OrdinalIgnoreCase)
                    );
                }

                // --------------------------------------------------------
                // TOTAL FILTERED RECORDS
                // --------------------------------------------------------
                var filteredList = filteredPurchases
                    .OrderByDescending(p => p.PurId)
                    .ToList();

                int totalRecords = filteredList.Count;

                // --------------------------------------------------------
                // TOTAL PAGES
                // --------------------------------------------------------
                int totalPages =
                    totalRecords == 0
                        ? 1
                        : (int)Math.Ceiling(
                            totalRecords / (double)pageSize);

                // --------------------------------------------------------
                // Prevent invalid page number
                // --------------------------------------------------------
                if (page > totalPages)
                {
                    page = totalPages;
                }

                // --------------------------------------------------------
                // PAGINATION
                // --------------------------------------------------------
                var paginatedData = filteredList
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // --------------------------------------------------------
                // VIEWBAG
                // --------------------------------------------------------
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.TotalRecords = totalRecords;
                ViewBag.CurrentSearch = searchTerm ?? string.Empty;
                ViewBag.PageSize = pageSize;

                // --------------------------------------------------------
                // STATISTICS
                // --------------------------------------------------------
                ViewBag.TotalPurchaseAmount = purchaseList.Sum(
                    p => p.GrandTotal);

                ViewBag.TotalNetAmount = purchaseList.Sum(
                    p => p.NetTotal);

                ViewBag.TotalDiscount = purchaseList.Sum(
                    p => p.Discount);

                ViewBag.LatestPurchaseDate =
                    purchaseList.Any()
                        ? purchaseList.Max(p => p.PurchaseDate)
                        : (DateTime?)null;

                // --------------------------------------------------------
                // IMPORTANT:
                // This view receives List<PurchaseMasterVM>
                // --------------------------------------------------------
                return View(paginatedData);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to load purchases: {ex.Message}";

                return View(new List<PurchaseMasterVM>());
            }
        }

        // ================================================================
        // CREATE - GET
        // GET: /Purchase/Create
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            try
            {
                await PopulateDropdownsAsync();

                var (
                    nextPurId,
                    nextInvoiceNo
                ) = await _purchaseService
                    .GetNextPurchaseNumberAsync();

                ViewBag.NextPurId = nextPurId;
                ViewBag.NextInvoiceNo = nextInvoiceNo;

                var vm = new PurchaseMasterVM
                {
                    PurId = nextPurId,
                    PurchaseDate = DateTime.Now,
                    InvoiceNo = nextInvoiceNo,
                    PurchaseDetails = new List<PurchaseDetailVM>()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to open purchase form: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        // ================================================================
        // CREATE - POST
        // POST: /Purchase/Create
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseMasterVM vm)
        {
            // ------------------------------------------------------------
            // Ensure collection is never null
            // ------------------------------------------------------------
            vm.PurchaseDetails ??= new List<PurchaseDetailVM>();

            // ------------------------------------------------------------
            // At least one purchase detail is required
            // ------------------------------------------------------------
            if (!vm.PurchaseDetails.Any())
            {
                ModelState.AddModelError(
                    "PurchaseDetails",
                    "Please add at least one medicine.");
            }

            // ------------------------------------------------------------
            // Remove calculated/display-only fields
            // ------------------------------------------------------------
            RemoveCalculatedFieldsFromModelState();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();

                return View(vm);
            }

            try
            {
                bool result =
                    await _purchaseService.CreatePurchaseAsync(vm);

                if (result)
                {
                    TempData["Success"] =
                        $"Purchase PUR-{vm.PurId:D4} created successfully.";

                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(
                    "",
                    "Purchase could not be created.");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"Error creating purchase: {ex.Message}");
            }

            await PopulateDropdownsAsync();

            return View(vm);
        }

        // ================================================================
        // DETAILS
        // GET: /Purchase/Details/5
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var vm =
                    await _purchaseService.GetByIdAsync(id);

                if (vm == null)
                {
                    return NotFound();
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to load purchase details: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        // ================================================================
        // EDIT - GET
        // GET: /Purchase/Edit/5
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var vm =
                    await _purchaseService.GetByIdAsync(id);

                if (vm == null)
                {
                    return NotFound();
                }

                vm.PurchaseDetails ??=
                    new List<PurchaseDetailVM>();

                await PopulateDropdownsAsync();

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to load purchase for editing: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        // ================================================================
        // EDIT - POST
        // POST: /Purchase/Edit
        // ================================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PurchaseMasterVM vm)
        {
            vm.PurchaseDetails ??=
                new List<PurchaseDetailVM>();

            // ------------------------------------------------------------
            // Validate details
            // ------------------------------------------------------------
            if (!vm.PurchaseDetails.Any())
            {
                ModelState.AddModelError(
                    "PurchaseDetails",
                    "Please add at least one medicine.");
            }

            // ------------------------------------------------------------
            // Remove calculated/display-only fields
            // ------------------------------------------------------------
            RemoveCalculatedFieldsFromModelState();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();

                return View(vm);
            }

            try
            {
                bool result =
                    await _purchaseService.UpdatePurchaseAsync(vm);

                if (result)
                {
                    TempData["Success"] =
                        $"Purchase PUR-{vm.PurId:D4} updated successfully.";

                    return RedirectToAction(nameof(Index));
                }

                TempData["Error"] =
                    "Purchase not found or could not be updated.";

                await PopulateDropdownsAsync();

                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"Error updating purchase: {ex.Message}");

                await PopulateDropdownsAsync();

                return View(vm);
            }
        }

        // ================================================================
        // DELETE - GET
        // GET: /Purchase/Delete/5
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                var vm =
                    await _purchaseService.GetByIdAsync(id);

                if (vm == null)
                {
                    return NotFound();
                }

                return View(vm);
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Unable to load purchase: {ex.Message}";

                return RedirectToAction(nameof(Index));
            }
        }

        // ================================================================
        // DELETE - POST
        // POST: /Purchase/Delete
        // ================================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid purchase ID.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool result =
                    await _purchaseService.DeleteAsync(id);

                if (result)
                {
                    TempData["Success"] =
                        $"Purchase PUR-{id:D4} deleted successfully.";
                }
                else
                {
                    TempData["Error"] =
                        "Purchase not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Error deleting purchase: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ================================================================
        // AJAX - MEDICINE INFORMATION
        // GET: /Purchase/GetMedicineInfo/5
        // ================================================================
        [HttpGet]
        public async Task<IActionResult> GetMedicineInfo(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        success = false,
                        message = "Invalid medicine ID."
                    });
            }

            try
            {
                var medicine =
                    await _medicineService.GetByIdAsync(id);

                if (medicine == null)
                {
                    return NotFound(
                        new
                        {
                            success = false,
                            message = "Medicine not found."
                        });
                }

                return Json(
                    new
                    {
                        success = true,
                        medId = medicine.MedId,
                        name = medicine.MedName,
                        rate = medicine.SaleRate,
                        stock = medicine.Stock
                    });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        // ================================================================
        // HELPER - DROPDOWNS
        // ================================================================
        private async Task PopulateDropdownsAsync()
        {
            var suppliers =
                await _supplierService.GetAllAsync()
                ?? Enumerable.Empty<SupplierVM>();

            var medicines =
                await _medicineService.GetAllAsync()
                ?? Enumerable.Empty<MedicineMasterVM>();

            ViewBag.Suppliers =
                new SelectList(
                    suppliers,
                    "SuppId",
                    "SuppName");

            ViewBag.Medicines =
                new SelectList(
                    medicines,
                    "MedId",
                    "MedName");
        }

        // ================================================================
        // HELPER - MODELSTATE
        // ================================================================
        private void RemoveCalculatedFieldsFromModelState()
        {
            // Master calculated fields
            ModelState.Remove(nameof(PurchaseMasterVM.GrandTotal));
            ModelState.Remove(nameof(PurchaseMasterVM.NetTotal));

            // Display-only master field
            ModelState.Remove(nameof(PurchaseMasterVM.SupplierName));

            // ------------------------------------------------------------
            // Remove calculated/display-only detail fields
            // ------------------------------------------------------------
            var keysToRemove = ModelState.Keys
                .Where(key =>
                    key.EndsWith(".Amt",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    key.EndsWith(".Total",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    key.EndsWith(".GstAmt",
                        StringComparison.OrdinalIgnoreCase)
                    ||
                    key.EndsWith(".MedicineName",
                        StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
        }
    }
}