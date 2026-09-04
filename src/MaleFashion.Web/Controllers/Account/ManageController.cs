using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Models.Account.Manage;

namespace MaleFashion.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UrlEncoder _urlEncoder;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _urlEncoder = urlEncoder;
        }

        public IActionResult Index()
        {

            return View();
        }

        #region Change Password
        public async Task<IActionResult> ChangePassword()
        {
            var model = new ChangePasswordModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your password has been changed.";
                return RedirectToAction("Login");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        #endregion

        #region Two-Factor Authentication
        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new TwoFactorAuthenticationModel
            {
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
            };

            await LoadSharedKeyAndQrCodeUriAsync(user, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate2fa(TwoFactorAuthenticationModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(model.Code))
            {
                ModelState.AddModelError("Code", "Verification code is required.");
                model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View("TwoFactorAuthentication", model);
            }

            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);

            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError("Code", "Verification code is invalid.");
                model.Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View("TwoFactorAuthentication", model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            TempData["SuccessMessage"] = "Two-factor authentication (2FA) has been successfully activated.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate2fa()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Unexpected error occurred deactivating 2FA.";
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            TempData["SuccessMessage"] = "Two-factor authentication (2FA) has been deactivated.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user, TwoFactorAuthenticationModel model)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.HasAuthenticator = !string.IsNullOrEmpty(unformattedKey);
            model.SharedKey = FormatKey(unformattedKey!);

            var email = await _userManager.GetEmailAsync(user);
            model.AuthenticatorUri = GenerateQrCodeUri(email ?? "User", unformattedKey!);
        }

        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.AsSpan(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                AuthenticatorUriFormat,
                _urlEncoder.Encode("MaleFashion"),
                _urlEncoder.Encode(email),
                unformattedKey);
        }
        #endregion
    }
}