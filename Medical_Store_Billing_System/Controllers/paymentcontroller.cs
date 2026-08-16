using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly ISupplierService _supplierService;

        public PaymentController(IPaymentService paymentService, ISupplierService supplierService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _supplierService = supplierService ?? throw new ArgumentNullException(nameof(supplierService));
        }

        // GET: /Payment/Index
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL payments from database
            var allPayments = await _paymentService.GetAllPaymentsAsync();

            // Store all payments for JavaScript search (client-side search across all pages)
            ViewBag.AllPayments = allPayments;

            // Apply server-side search filter
            IEnumerable<PaymentVM> filteredPayments = allPayments;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredPayments = filteredPayments.Where(x =>
                    x.PaymentId.ToString().Contains(searchTerm) ||
                    (!string.IsNullOrEmpty(x.SupplierName) &&
                     x.SupplierName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.PaymentDate.ToString("dd-MM-yyyy").Contains(searchTerm) ||
                    x.Amount.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredPayments.Count();

            // Apply pagination
            var paginatedData = filteredPayments
                .OrderByDescending(p => p.PaymentId)
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
            ViewBag.TotalAmount = allPayments.Sum(p => p.Amount);

            return View(paginatedData);
        }

        // GET: /Payment/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }

        // GET: /Payment/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new PaymentVM { PaymentDate = DateTime.Now });
        }

        // POST: /Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentVM paymentVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(paymentVM);
            }

            try
            {
                if (await _paymentService.CreatePaymentAsync(paymentVM))
                {
                    TempData["Success"] = "Payment saved successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(paymentVM);
        }

        // GET: /Payment/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync();
            return View(payment);
        }

        // POST: /Payment/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PaymentVM paymentVM)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(paymentVM);
            }

            try
            {
                if (await _paymentService.UpdatePaymentAsync(paymentVM))
                {
                    TempData["Success"] = "Payment updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["Error"] = "Failed to update payment.";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(paymentVM);
        }

        // GET: /Payment/Delete/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                TempData["Error"] = "Payment not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(payment);
        }

        // POST: /Payment/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // 🔧 FIX: Use the correct method name from your IPaymentService
                // Option 1: If the method is named DeletePaymentAsync
                // if (await _paymentService.DeletePaymentAsync(id))

                // Option 2: If the method is named DeleteAsync (most common)
                if (await _paymentService.DeleteAsync(id))
                {
                    TempData["Success"] = "Payment deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Payment not found or could not be deleted.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Payment/GetSupplierBalance/{id}  (called via AJAX on Create/Edit form)
        [HttpGet]
        public async Task<IActionResult> GetSupplierBalance(int id)
        {
            var suppliers = await _supplierService.GetAllAsync();
            var supplier = suppliers.FirstOrDefault(s => s.SuppId == id);
            if (supplier == null)
                return NotFound();

            return Json(new { balance = supplier.SuppBal });
        }

        // ── helpers ────────────────────────────────────────────────────────
        private async Task PopulateDropdownsAsync()
        {
            var suppliers = await _supplierService.GetAllAsync();
            ViewBag.Suppliers = new SelectList(suppliers, "SuppId", "SuppName");
        }
    }
}