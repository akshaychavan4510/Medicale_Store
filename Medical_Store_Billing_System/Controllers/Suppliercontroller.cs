// Controllers/SupplierController.cs
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class SupplierController : Controller
    {
        private readonly ISupplierService _service;

        public SupplierController(ISupplierService service) => _service = service;

        // GET: /Supplier/Index
        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL suppliers from database
            var allSuppliers = await _service.GetAllAsync();

            // Store all suppliers for JavaScript search (client-side search across all pages)
            ViewBag.AllSuppliers = allSuppliers;

            // Apply server-side search filter
            IEnumerable<SupplierVM> filteredSuppliers = allSuppliers;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredSuppliers = filteredSuppliers.Where(x =>
                    (!string.IsNullOrEmpty(x.SuppName) &&
                     x.SuppName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.SuppPhone) &&
                     x.SuppPhone.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.SuppEmail) &&
                     x.SuppEmail.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.GstNo) &&
                     x.GstNo.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.SuppId.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredSuppliers.Count();

            // Apply pagination
            var paginatedData = filteredSuppliers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Set ViewBag properties
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.PageSize = pageSize;

            // Calculate total balance for display
            ViewBag.TotalBalance = allSuppliers.Sum(s => s.SuppBal);

            return View(paginatedData);
        }

        // GET: /Supplier/Create
        public IActionResult Create() => View();

        // POST: /Supplier/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SupplierVM vm)
        {
            if (!ModelState.IsValid) return View(vm);
            try
            {
                await _service.CreateAsync(vm);
                TempData["Success"] = "Supplier created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        // GET: /Supplier/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: /Supplier/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SupplierVM vm)
        {
            if (id != vm.SuppId) return BadRequest();
            if (!ModelState.IsValid) return View(vm);
            try
            {
                await _service.UpdateAsync(vm);
                TempData["Success"] = "Supplier updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(vm);
            }
        }

        // GET: /Supplier/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // GET: /Supplier/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        // POST: /Supplier/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                if (await _service.DeleteAsync(id))
                    TempData["Success"] = "Supplier deleted successfully.";
                else
                    TempData["Error"] = "Supplier not found.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}