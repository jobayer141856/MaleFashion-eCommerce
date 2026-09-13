using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
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
            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("OrderManager") || User.IsInRole("CatalogManager") || User.IsInRole("BlogEditor") || User.IsInRole("SupportStaff")))
            {
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }
            return RedirectToAction(nameof(Login));
        }

        #region Login & Logout

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string? returnUrl = null)
        {
            returnUrl ??= Url.Action("Index", "Home", new { area = "Admin" });

            if (User.Identity?.IsAuthenticated == true && (User.IsInRole("Admin") || User.IsInRole("SuperAdmin") || User.IsInRole("OrderManager") || User.IsInRole("CatalogManager") || User.IsInRole("BlogEditor") || User.IsInRole("SupportStaff")))
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home", new { area = "Admin" });
            }

            var model = new LoginModel();

            // Clear any existing external cookies
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            model.ReturnUrl = returnUrl;
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
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

                // Verify the user has an administrative or operational role
                var userRoles = await _userManager.GetRolesAsync(user);
                var isStaffOrAdmin = userRoles.Any(r =>
                    r.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("OrderManager", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("CatalogManager", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("BlogEditor", StringComparison.OrdinalIgnoreCase) ||
                    r.Equals("SupportStaff", StringComparison.OrdinalIgnoreCase));

                if (!isStaffOrAdmin)
                {
                    ModelState.AddModelError(string.Empty, "Access denied. Only authorized staff and administrators can sign in here.");
                    return View(model);
                }

                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Admin user logged in.");
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return LocalRedirect(model.ReturnUrl);
                    }
                    return RedirectToAction("Index", "Home", new { area = "Admin" });
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string? returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Admin user logged out successfully.");

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("Admin user logged out via GET.");
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        #endregion

        #region Change Password

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,OrderManager,CatalogManager,BlogEditor,SupportStaff")]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            return View(new AdminChangePasswordModel());
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,OrderManager,CatalogManager,BlogEditor,SupportStaff")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(AdminChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                _logger.LogInformation("Admin user successfully updated their password.");

                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Your password has been changed successfully.",
                    Type = ResponseTypes.Success
                });

                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            TempData.Put(Constants.ResponseTempKey, new ResponseModel
            {
                Message = "Failed to change password. Please check the errors below.",
                Type = ResponseTypes.Danger
            });

            return View(model);
        }

        #endregion
    }
}