using Medical_Store_Billing_System.Models;
using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _service;

        public CustomerController(ICustomerService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL customers from database
            var allCustomers = await _service.GetAllAsync();

            // Store all customers for JavaScript search (client-side search)
            ViewBag.AllCustomers = allCustomers;

            // Apply server-side search filter
            IEnumerable<CustomerVM> filteredCustomers = allCustomers;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredCustomers = filteredCustomers.Where(x =>
                    (!string.IsNullOrEmpty(x.CustName) &&
                     x.CustName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.CustPhone) &&
                     x.CustPhone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.CustEmail) &&
                     x.CustEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.CustId.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredCustomers.Count();

            // Apply pagination
            var paginatedData = filteredCustomers
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
            ViewBag.TotalBalance = allCustomers.Sum(c => c.CustBal);
            ViewBag.AvgBalance = allCustomers.Any() ? allCustomers.Average(c => c.CustBal) : 0;
            ViewBag.MaxBalance = allCustomers.Any() ? allCustomers.Max(c => c.CustBal) : 0;

            return View(paginatedData);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var id = await _service.CreateAsync(vm);
                TempData["Success"] = "Customer created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create customer: " + ex.Message);
            }

            return View(vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null)
                return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerVM vm)
        {
            if (id != vm.CustId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                if (await _service.UpdateAsync(vm))
                {
                    TempData["Success"] = "Customer updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update customer: " + ex.Message);
            }

            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null)
                return NotFound();

            return View(vm);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null)
                return NotFound();

            return View(vm);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (await _service.DeleteAsync(id))
                {
                    TempData["Success"] = "Customer deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Customer not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete customer: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}