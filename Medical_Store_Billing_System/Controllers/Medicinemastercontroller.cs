using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class MedicineMasterController : Controller
    {
        private readonly IMedicineMasterService _service;
        private readonly IMedicineCategoryService _categoryService;
        private readonly IBrandService _brandService;

        public MedicineMasterController(
            IMedicineMasterService service,
            IMedicineCategoryService categoryService,
            IBrandService brandService)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _brandService = brandService ?? throw new ArgumentNullException(nameof(brandService));
        }

        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL medicines from database
            var allMedicines = await _service.GetAllAsync();

            // Store all medicines for JavaScript search (client-side search across all pages)
            ViewBag.AllMedicines = allMedicines;

            // Apply server-side search filter
            IEnumerable<MedicineMasterVM> filteredMedicines = allMedicines;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredMedicines = filteredMedicines.Where(x =>
                    (!string.IsNullOrEmpty(x.MedNm) &&
                     x.MedNm.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.CategoryName) &&
                     x.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(x.BrandName) &&
                     x.BrandName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.MedId.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredMedicines.Count();

            // Apply pagination
            var paginatedData = filteredMedicines
                .OrderBy(m => m.MedId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Set ViewBag properties
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.PageSize = pageSize;

            return View(paginatedData);
        }

        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineMasterVM vm)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            try
            {
                var id = await _service.CreateAsync(vm);
                TempData["Success"] = "Medicine created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create medicine: " + ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(vm);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return NotFound();

            await PopulateDropdownsAsync();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MedicineMasterVM vm)
        {
            if (id != vm.MedId) return BadRequest();

            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync();
                return View(vm);
            }

            try
            {
                if (await _service.UpdateAsync(vm))
                {
                    TempData["Success"] = "Medicine updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update medicine: " + ex.Message);
            }

            await PopulateDropdownsAsync();
            return View(vm);
        }

        public async Task<IActionResult> Details(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var vm = await _service.GetByIdAsync(id);
            if (vm == null) return NotFound();
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
                    TempData["Success"] = "Medicine deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Medicine not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete medicine: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // AJAX: Get medicine rate by ID
        [HttpGet]
        public async Task<IActionResult> GetMedicineRate(int id)
        {
            var med = await _service.GetByIdAsync(id);
            if (med == null) return NotFound();
            return Json(new { rate = med.SaleRate, stock = med.Stock, name = med.MedName });
        }

        private async Task PopulateDropdownsAsync()
        {
            var categories = await _categoryService.GetAllAsync();
            var brands = await _brandService.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
            ViewBag.Brands = new SelectList(brands, "BrandId", "BrandName");
        }
    }
}