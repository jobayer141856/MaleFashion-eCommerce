using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Areas.Admin.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MaleFashion.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            var model = new LoginModel();
            returnUrl ??= Url.Action("Index", "Home", new { area = "Admin" });

            // Clear any existing external cookies
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            model.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            model.ReturnUrl ??= Url.Action("Index", "Home", new { area = "Admin" });

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid credentials.");
                    return View(model);
                }

                // Verify the user has the "Admin" role
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                if (!isAdmin)
                {
                    ModelState.AddModelError(string.Empty, "Access denied. Only administrators can sign in here.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user logged in.");
                    return LocalRedirect(model.ReturnUrl);
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Account locked out.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }

            return View(model);
        }
    }
}