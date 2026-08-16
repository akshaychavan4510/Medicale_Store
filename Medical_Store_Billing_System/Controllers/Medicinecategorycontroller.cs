using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class MedicineCategoryController : Controller
    {
        private readonly IMedicineCategoryService _service;

        public MedicineCategoryController(IMedicineCategoryService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;

            // Get ALL categories from database
            var allCategories = await _service.GetAllAsync();

            // Store all categories for JavaScript search (client-side search across all pages)
            ViewBag.AllCategories = allCategories;

            // Apply server-side search filter
            IEnumerable<MedicineCategoryVM> filteredCategories = allCategories;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredCategories = filteredCategories.Where(x =>
                    (!string.IsNullOrEmpty(x.CatName) &&
                     x.CatName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.CatId.ToString().Contains(searchTerm)
                );
            }

            // Get total records for pagination
            int totalRecords = filteredCategories.Count();

            // Apply pagination
            var paginatedData = filteredCategories
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MedicineCategoryVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var id = await _service.CreateAsync(vm);
                TempData["Success"] = "Category created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create category: " + ex.Message);
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
        public async Task<IActionResult> Edit(int id, MedicineCategoryVM vm)
        {
            if (id != vm.CatId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                if (await _service.UpdateAsync(vm))
                {
                    TempData["Success"] = "Category updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update category: " + ex.Message);
            }

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
                    TempData["Success"] = "Category deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Category not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete category: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}