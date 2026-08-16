// Controllers/ReceiptController.cs
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

        public ReceiptController(IReceiptService receiptService, ICustomerService customerService)
        {
            _receiptService = receiptService ?? throw new ArgumentNullException(nameof(receiptService));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        // GET: /Receipt/Index
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL receipts from database
            var allReceipts = await _receiptService.GetAllReceiptsAsync();

            // Store all receipts for JavaScript search (client-side search across all pages)
            ViewBag.AllReceipts = allReceipts;

            // Apply server-side search filter
            IEnumerable<ReceiptVM> filteredReceipts = allReceipts;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredReceipts = filteredReceipts.Where(x =>
                    x.ReceiptId.ToString().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(x.CustomerName) &&
                     x.CustomerName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.ReceiptDate.ToString("dd-MM-yyyy").Contains(searchTerm) ||
                    x.Amount.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredReceipts.Count();

            // Apply pagination
            var paginatedData = filteredReceipts
                .OrderByDescending(r => r.ReceiptId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Set ViewBag properties
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.PageSize = pageSize;

            // Calculate total amount for display
            ViewBag.TotalAmount = allReceipts.Sum(r => r.Amount);

            return View(paginatedData);
        }

        // GET: /Receipt/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var receipt = await _receiptService.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        // GET: /Receipt/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new ReceiptVM { ReceiptDate = DateTime.Now });
        }

        // POST: /Receipt/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiptVM receiptVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(receiptVM);
            }

            try
            {
                receiptVM.CreatedBy = User.Identity?.Name;
                await _receiptService.CreateReceiptAsync(receiptVM);
                TempData["Success"] = "Receipt created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(receiptVM);
        }

        // GET: /Receipt/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var receipt = await _receiptService.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound();
            await PopulateDropdownsAsync(receipt.CustId);
            return View(receipt);
        }

        // POST: /Receipt/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReceiptVM receiptVM)
        {
            if (id != receiptVM.ReceiptId) return BadRequest();
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(receiptVM.CustId);
                return View(receiptVM);
            }

            try
            {
                await _receiptService.UpdateReceiptAsync(receiptVM);
                TempData["Success"] = "Receipt updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateDropdownsAsync(receiptVM.CustId);
            return View(receiptVM);
        }

        // GET: /Receipt/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var receipt = await _receiptService.GetReceiptByIdAsync(id);
            if (receipt == null) return NotFound();
            return View(receipt);
        }

        // POST: /Receipt/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (await _receiptService.DeleteAsync(id))
                    TempData["Success"] = "Receipt deleted successfully.";
                else
                    TempData["Error"] = "Receipt not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get customer outstanding balance
        [HttpGet]
        public async Task<IActionResult> GetCustomerBalance(int id)
        {
            var customers = await _customerService.GetAllAsync();
            var customer = customers.FirstOrDefault(c => c.CustId == id);
            if (customer == null) return NotFound();
            return Json(new { balance = customer.CustBal, name = customer.CustName });
        }

        private async Task PopulateDropdownsAsync(int? selectedCustId = null)
        {
            var customers = await _customerService.GetAllAsync();
            ViewBag.Customers = new SelectList(customers, "CustId", "CustName", selectedCustId);
        }
    }
}