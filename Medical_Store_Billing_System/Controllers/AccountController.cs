using MedicalStore.Business.ViewModels;
using Medical_Store_Billing_System.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Medical_Store_Billing_System.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<AccountController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        // ── GET /Account/Login ────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]   // ← CRITICAL: Login page must never require auth
        public IActionResult Login(string? returnUrl = null)
        {
            // Already signed in → go to dashboard
            if (_signInManager.IsSignedIn(User))
                return RedirectToAction("Index", "Home");

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // ── POST /Account/Login ───────────────────────────────────────
        [HttpPost]
        [AllowAnonymous]   // ← CRITICAL: Login POST must never require auth
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM vm, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(vm);

            var result = await _signInManager.PasswordSignInAsync(
                userName: vm.Email,
                password: vm.Password,
                isPersistent: vm.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in successfully.", vm.Email);

                // Also set session so any legacy Session["User"] checks work
                var user = await _userManager.FindByEmailAsync(vm.Email);
                HttpContext.Session.SetString("User", user?.FullName ?? vm.Email);

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} account is locked out.", vm.Email);
                ModelState.AddModelError(string.Empty, "Account is locked out. Please try again later.");
                return View(vm);
            }

            _logger.LogWarning("Failed login attempt for {Email}.", vm.Email);
            ModelState.AddModelError(string.Empty, "Invalid Email or Password.");
            return View(vm);
        }

        // ── POST /Account/Logout ──────────────────────────────────────
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Login", "Account");
        }

        // ── GET /Account/AccessDenied ─────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}