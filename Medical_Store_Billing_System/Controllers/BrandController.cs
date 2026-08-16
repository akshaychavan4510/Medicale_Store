using MedicalStore.Business.Interfaces;
using MedicalStore.Business.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStore.Web.Controllers
{
    [Authorize]
    public class BrandController : Controller
    {
        private readonly IBrandService _service;

        public BrandController(IBrandService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<IActionResult> Index(string searchTerm = "", int page = 1)
        {
            const int pageSize = 10;
            var allBrands = await _service.GetAllAsync();
            ViewBag.AllBrands = allBrands;

            IEnumerable<BrandVM> filteredBrands = allBrands;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.Trim();
                filteredBrands = filteredBrands.Where(x =>
                    (!string.IsNullOrEmpty(x.BrandName) &&
                     x.BrandName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                    x.BrandId.ToString().Contains(searchTerm)
                );
            }

            int totalRecords = filteredBrands.Count();
            var paginatedData = filteredBrands
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            ViewBag.TotalRecords = totalRecords;
            ViewBag.CurrentSearch = searchTerm;

            return View(paginatedData);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BrandVM vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var id = await _service.CreateAsync(vm);
                TempData["Success"] = "Brand created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to create brand: " + ex.Message);
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
        public async Task<IActionResult> Edit(int id, BrandVM vm)
        {
            if (id != vm.BrandId)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                if (await _service.UpdateAsync(vm))
                {
                    TempData["Success"] = "Brand updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Failed to update brand: " + ex.Message);
            }

            return View(vm);
        }

        // GET: /Brand/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var details = await _service.GetBrandDetailsAsync(id);
            if (details == null)
                return NotFound();

            return View(details);
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
                    TempData["Success"] = "Brand deleted successfully.";
                }
                else
                {
                    TempData["Error"] = "Brand not found.";
                }
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Failed to delete brand: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}