using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalStore.Web.Controllers
{
    [Authorize]   // ← redirects to /Account/Login if not signed in (no redirect loop)
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Identity is already authenticated here (enforced by [Authorize] above)
            ViewBag.UserName = User.Identity?.Name ?? "Admin";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}