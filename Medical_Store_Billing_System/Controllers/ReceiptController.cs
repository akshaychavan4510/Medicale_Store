using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class ReceiptController : Controller
    {
        private readonly IReceiptService _receiptService;
        private readonly ICustomerService _customerService;

        public ReceiptController(
            IReceiptService receiptService,
            ICustomerService customerService)
        {
            _receiptService = receiptService
                ?? throw new ArgumentNullException(nameof(receiptService));

            _customerService = customerService
                ?? throw new ArgumentNullException(nameof(customerService));
        }

        // ============================================================
        // INDEX
        // GET: /Receipt/Index
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Index(
            string searchTerm = "",
            int page = 1)
        {
            const int pageSize = 10;

            // Prevent invalid page numbers
            if (page < 1)
            {
                page = 1;
            }

            // --------------------------------------------------------
            // GET ALL RECEIPTS
            // --------------------------------------------------------
            var allReceipts =
                (await _receiptService.GetAllReceiptsAsync())
                ?.ToList()
                ?? new List<ReceiptVM>();

            // --------------------------------------------------------
            // Keep all records for client-side search
            // --------------------------------------------------------
            ViewBag.AllReceipts = allReceipts;

            // --------------------------------------------------------
            // SERVER-SIDE SEARCH
            // --------------------------------------------------------
            IEnumerable<ReceiptVM> filteredReceipts = allReceipts;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();

                filteredReceipts = filteredReceipts.Where(r =>
                    // Receipt ID
                    r.ReceiptId
                        .ToString()
                        .Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase)

                    // Customer Name
                    || (!string.IsNullOrWhiteSpace(r.CustomerName)
                        && r.CustomerName.Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase))

                    // Date
                    || r.ReceiptDate
                        .ToString("dd-MM-yyyy")
                        .Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase)

                    // Amount
                    || r.Amount
                        .ToString("0.00")
                        .Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase)

                    // Payment Mode
                    || (!string.IsNullOrWhiteSpace(r.PayMode)
                        && r.PayMode.Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase))

                    // Reference Number
                    || (!string.IsNullOrWhiteSpace(r.RefNo)
                        && r.RefNo.Contains(
                            searchTerm,
                            StringComparison.OrdinalIgnoreCase))
                );
            }

            // --------------------------------------------------------
            // TOTAL FILTERED RECORDS
            // --------------------------------------------------------
            int totalRecords = filteredReceipts.Count();

            // --------------------------------------------------------
            // TOTAL PAGES
            // --------------------------------------------------------
            int totalPages = totalRecords == 0
                ? 1
                : (int)Math.Ceiling(
                    totalRecords / (double)pageSize);

            // Prevent page > total pages
            if (page > totalPages)
            {
                page = totalPages;
            }

            // --------------------------------------------------------
            // PAGINATION
            // --------------------------------------------------------
            var paginatedData = filteredReceipts
                .OrderByDescending(r => r.ReceiptId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // --------------------------------------------------------
            // VIEWBAG VALUES
            // --------------------------------------------------------
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm ?? "";
            ViewBag.PageSize = pageSize;

            // --------------------------------------------------------
            // TOTAL RECEIPT AMOUNT
            // All receipts, not only current page
            // --------------------------------------------------------
            ViewBag.TotalAmount = allReceipts.Sum(r => r.Amount);

            // --------------------------------------------------------
            // CURRENT PAGE AMOUNT
            // --------------------------------------------------------
            ViewBag.CurrentPageAmount =
                paginatedData.Sum(r => r.Amount);

            // --------------------------------------------------------
            // LATEST RECEIPT DATE
            // --------------------------------------------------------
            ViewBag.LatestReceiptDate =
                allReceipts.Any()
                    ? allReceipts.Max(r => r.ReceiptDate)
                    : (DateTime?)null;

            // IMPORTANT:
            // Receipt/Index.cshtml MUST contain:
            //
            // @model IEnumerable<MedicalStore.Business.ViewModels.ReceiptVM>
            //
            return View(paginatedData);
        }

        // ============================================================
        // DETAILS
        // GET: /Receipt/Details/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var receipt =
                await _receiptService.GetReceiptByIdAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            return View(receipt);
        }

        // ============================================================
        // CREATE - GET
        // GET: /Receipt/Create
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();

            var vm = new ReceiptVM
            {
                ReceiptDate = DateTime.Now,
                PayMode = "Cash"
            };

            return View(vm);
        }

        // ============================================================
        // CREATE - POST
        // POST: /Receipt/Create
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptVM receiptVM)
        {
            if (receiptVM == null)
            {
                return BadRequest();
            }

            // --------------------------------------------------------
            // Validate Model
            // --------------------------------------------------------
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(receiptVM.CustId);
                return View(receiptVM);
            }

            try
            {
                // ----------------------------------------------------
                // Logged-in user
                // ----------------------------------------------------
                receiptVM.CreatedBy =
                    User.Identity?.IsAuthenticated == true
                        ? User.Identity.Name
                        : "System";

                // ----------------------------------------------------
                // Default payment mode
                // ----------------------------------------------------
                if (string.IsNullOrWhiteSpace(receiptVM.PayMode))
                {
                    receiptVM.PayMode = "Cash";
                }

                // ----------------------------------------------------
                // Create Receipt
                // ----------------------------------------------------
                await _receiptService.CreateReceiptAsync(receiptVM);

                TempData["Success"] =
                    $"Receipt RCP-{receiptVM.ReceiptId:D4} created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"Unable to create receipt: {ex.Message}");
            }

            await PopulateDropdownsAsync(receiptVM.CustId);

            return View(receiptVM);
        }

        // ============================================================
        // EDIT - GET
        // GET: /Receipt/Edit/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var receipt =
                await _receiptService.GetReceiptByIdAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            await PopulateDropdownsAsync(receipt.CustId);

            return View(receipt);
        }

        // ============================================================
        // EDIT - POST
        // POST: /Receipt/Edit/5
        // ============================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ReceiptVM receiptVM)
        {
            if (receiptVM == null)
            {
                return BadRequest();
            }

            // --------------------------------------------------------
            // Route ID must match model ID
            // --------------------------------------------------------
            if (id != receiptVM.ReceiptId)
            {
                return BadRequest();
            }

            // --------------------------------------------------------
            // Model validation
            // --------------------------------------------------------
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(receiptVM.CustId);
                return View(receiptVM);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(receiptVM.PayMode))
                {
                    receiptVM.PayMode = "Cash";
                }

                await _receiptService.UpdateReceiptAsync(receiptVM);

                TempData["Success"] =
                    $"Receipt RCP-{receiptVM.ReceiptId:D4} updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    $"Unable to update receipt: {ex.Message}");
            }

            await PopulateDropdownsAsync(receiptVM.CustId);

            return View(receiptVM);
        }

        // ============================================================
        // DELETE - GET
        // GET: /Receipt/Delete/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var receipt =
                await _receiptService.GetReceiptByIdAsync(id);

            if (receipt == null)
            {
                return NotFound();
            }

            return View(receipt);
        }

        // ============================================================
        // DELETE - POST
        // POST: /Receipt/Delete/5
        // ============================================================
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (id <= 0)
            {
                TempData["Error"] = "Invalid receipt ID.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                bool deleted =
                    await _receiptService.DeleteAsync(id);

                if (deleted)
                {
                    TempData["Success"] =
                        $"Receipt RCP-{id:D4} deleted successfully.";
                }
                else
                {
                    TempData["Error"] =
                        "Receipt not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] =
                    $"Error deleting receipt: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // AJAX - CUSTOMER BALANCE
        // GET: /Receipt/GetCustomerBalance/5
        // ============================================================
        [HttpGet]
        public async Task<IActionResult> GetCustomerBalance(int id)
        {
            if (id <= 0)
            {
                return BadRequest(
                    new
                    {
                        success = false,
                        message = "Invalid customer ID."
                    });
            }

            try
            {
                var customers =
                    await _customerService.GetAllAsync();

                var customer =
                    customers.FirstOrDefault(
                        c => c.CustId == id);

                if (customer == null)
                {
                    return NotFound(
                        new
                        {
                            success = false,
                            message = "Customer not found."
                        });
                }

                return Json(new
                {
                    success = true,
                    balance = customer.CustBal,
                    name = customer.CustName
                });
            }
            catch (Exception ex)
            {
                return StatusCode(
                    500,
                    new
                    {
                        success = false,
                        message = ex.Message
                    });
            }
        }

        // ============================================================
        // HELPER - CUSTOMER DROPDOWN
        // ============================================================
        private async Task PopulateDropdownsAsync(
            int? selectedCustId = null)
        {
            var customers =
                (await _customerService.GetAllAsync())
                ?.ToList()
                ?? new List<CustomerVM>();

            ViewBag.Customers = new SelectList(
                customers,
                "CustId",
                "CustName",
                selectedCustId);
        }
    }
}