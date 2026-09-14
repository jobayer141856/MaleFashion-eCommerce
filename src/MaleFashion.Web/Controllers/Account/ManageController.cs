using Cortex.Mediator;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Entities.Reviews;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Models.Account.Manage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<ManageController> _logger;
        private readonly UrlEncoder _urlEncoder;

        public ManageController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IApplicationUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<ManageController> logger,
            UrlEncoder urlEncoder)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
            _urlEncoder = urlEncoder;
        }

        #region Helper Methods

        private async Task<Customer> GetOrCreateCustomerAsync(ApplicationUser user, CancellationToken cancellationToken = default)
        {
            var customer = await _unitOfWork.CustomerRepository.GetCustomerByUserIdAsync(user.Id, cancellationToken);
            if (customer == null)
            {
                customer = new Customer
                {
                    Id = IdentityGenerator.NewSequentialGuid(),
                    UserId = user.Id,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Origin = CustomerOrigin.Self,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.CustomerRepository.AddAsync(customer, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);
            }

            return customer;
        }

        private static CustomerAddressViewModel MapAddressToViewModel(CustomerAddress a)
        {
            return new CustomerAddressViewModel
            {
                Id = a.Id,
                CustomerId = a.CustomerId,
                Type = a.Type,
                StreetAddress = a.Address?.StreetAddress ?? string.Empty,
                ApartmentSuite = a.Address?.ApartmentSuite,
                City = a.Address?.City ?? string.Empty,
                State = a.Address?.State ?? string.Empty,
                PostalCode = a.Address?.PostalCode ?? string.Empty,
                Country = a.Address?.Country ?? "Bangladesh",
                IsDefault = a.IsDefault
            };
        }

        #endregion

        #region 1. Dashboard & Profile Management

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var shippingAddress = customer.Addresses.FirstOrDefault(a => a.IsDefault && a.Type == AddressType.Shipping)
                                  ?? customer.Addresses.FirstOrDefault(a => a.IsDefault)
                                  ?? customer.Addresses.FirstOrDefault();

            var billingAddress = customer.Addresses.FirstOrDefault(a => a.IsDefault && a.Type == AddressType.Billing)
                                 ?? customer.Addresses.FirstOrDefault(a => a.Type == AddressType.Billing);

            var recentOrders = customer.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new CustomerOrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount?.Amount ?? 0,
                    TotalItems = o.OrderItems.Sum(i => i.Quantity),
                    ItemNames = o.OrderItems.Select(i => i.ProductName).ToList()
                }).ToList();

            var model = new ManageModel
            {
                CustomerId = customer.Id,
                Email = user.Email,
                FirstName = !string.IsNullOrWhiteSpace(customer.FirstName) ? customer.FirstName : user.FirstName,
                LastName = !string.IsNullOrWhiteSpace(customer.LastName) ? customer.LastName : user.LastName,
                PhoneNumber = !string.IsNullOrWhiteSpace(customer.PhoneNumber) ? customer.PhoneNumber : user.PhoneNumber,
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                IsEmailConfirmed = user.EmailConfirmed,
                DefaultShippingAddress = shippingAddress != null ? MapAddressToViewModel(shippingAddress) : null,
                DefaultBillingAddress = billingAddress != null ? MapAddressToViewModel(billingAddress) : null,
                Addresses = customer.Addresses.Select(MapAddressToViewModel).ToList(),
                RecentOrders = recentOrders,
                TotalOrders = customer.Orders.Count,
                TotalWishlistItems = customer.Wishlist.Count,
                TotalReviews = customer.Reviews.Count
            };

            ViewData["ActiveMenu"] = "ManageAccount";
            ViewData["UserDisplayName"] = $"{model.FirstName} {model.LastName}".Trim();
            if (string.IsNullOrWhiteSpace(ViewData["UserDisplayName"]?.ToString()))
            {
                ViewData["UserDisplayName"] = user.UserName;
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var model = new CustomerEditProfileModel
            {
                FirstName = !string.IsNullOrWhiteSpace(customer.FirstName) ? customer.FirstName : (user.FirstName ?? string.Empty),
                LastName = !string.IsNullOrWhiteSpace(customer.LastName) ? customer.LastName : (user.LastName ?? string.Empty),
                Email = user.Email ?? string.Empty,
                PhoneNumber = !string.IsNullOrWhiteSpace(customer.PhoneNumber) ? customer.PhoneNumber : (user.PhoneNumber ?? string.Empty),
                DateOfBirth = customer.DateOfBirth,
                Gender = customer.Gender,
                IsEmailConfirmed = user.EmailConfirmed
            };

            ViewData["ActiveMenu"] = "MyProfile";
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(CustomerEditProfileModel model, CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyProfile";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            // Synchronize ApplicationUser
            user.FirstName = model.FirstName.Trim();
            user.LastName = model.LastName.Trim();
            user.PhoneNumber = model.PhoneNumber.Trim();
            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var err in updateResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, err.Description);
                }
                return View(model);
            }

            // Synchronize Customer Entity
            customer.FirstName = model.FirstName.Trim();
            customer.LastName = model.LastName.Trim();
            customer.PhoneNumber = model.PhoneNumber.Trim();
            customer.DateOfBirth = model.DateOfBirth;
            customer.Gender = model.Gender;
            customer.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync(cancellationToken);

            TempData["SuccessMessage"] = "Your profile information has been successfully updated.";
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region 2. Address Book Management

        [HttpGet]
        public async Task<IActionResult> AddressBook(CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);
            var addresses = customer.Addresses
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .Select(MapAddressToViewModel)
                .ToList();

            ViewData["ActiveMenu"] = "AddressBook";
            ViewBag.CustomerId = customer.Id;
            return View(addresses);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(CustomerAddressViewModel model, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to add address. Please verify all required fields.";
                return RedirectToAction(nameof(AddressBook));
            }

            // If new address is marked default, unset existing default of the same type
            if (model.IsDefault)
            {
                foreach (var existing in customer.Addresses.Where(a => a.Type == model.Type))
                {
                    existing.IsDefault = false;
                }
            }

            var address = new CustomerAddress
            {
                CustomerId = customer.Id,
                Type = model.Type,
                IsDefault = model.IsDefault || !customer.Addresses.Any(a => a.Type == model.Type),
                Address = new Address(
                    model.StreetAddress.Trim(),
                    model.ApartmentSuite?.Trim(),
                    model.City.Trim(),
                    model.State.Trim(),
                    model.PostalCode.Trim(),
                    string.IsNullOrWhiteSpace(model.Country) ? "Bangladesh" : model.Country.Trim()
                ),
                CreatedAt = DateTime.UtcNow
            };

            customer.Addresses.Add(address);
            await _unitOfWork.SaveAsync(cancellationToken);

            TempData["SuccessMessage"] = "New delivery address successfully added.";
            return RedirectToAction(nameof(AddressBook));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAddress(CustomerAddressViewModel model, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            if (!model.Id.HasValue || model.Id.Value == Guid.Empty)
            {
                TempData["ErrorMessage"] = "Invalid address identifier.";
                return RedirectToAction(nameof(AddressBook));
            }

            var address = customer.Addresses.FirstOrDefault(a => a.Id == model.Id.Value);
            if (address == null)
            {
                TempData["ErrorMessage"] = "Address not found or access denied.";
                return RedirectToAction(nameof(AddressBook));
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Failed to update address. Please verify required fields.";
                return RedirectToAction(nameof(AddressBook));
            }

            if (model.IsDefault && !address.IsDefault)
            {
                foreach (var other in customer.Addresses.Where(a => a.Type == model.Type && a.Id != address.Id))
                {
                    other.IsDefault = false;
                }
            }

            address.Type = model.Type;
            address.IsDefault = model.IsDefault;
            address.UpdatedAt = DateTime.UtcNow;

            if (address.Address == null)
            {
                address.Address = new Address(
                    model.StreetAddress.Trim(),
                    model.ApartmentSuite?.Trim(),
                    model.City.Trim(),
                    model.State.Trim(),
                    model.PostalCode.Trim(),
                    string.IsNullOrWhiteSpace(model.Country) ? "Bangladesh" : model.Country.Trim()
                );
            }
            else
            {
                address.Address.StreetAddress = model.StreetAddress.Trim();
                address.Address.ApartmentSuite = model.ApartmentSuite?.Trim();
                address.Address.City = model.City.Trim();
                address.Address.State = model.State.Trim();
                address.Address.PostalCode = model.PostalCode.Trim();
                address.Address.Country = string.IsNullOrWhiteSpace(model.Country) ? "Bangladesh" : model.Country.Trim();
            }

            await _unitOfWork.SaveAsync(cancellationToken);

            TempData["SuccessMessage"] = "Address successfully updated.";
            return RedirectToAction(nameof(AddressBook));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAddress(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);
            var address = customer.Addresses.FirstOrDefault(a => a.Id == id);

            if (address == null)
            {
                TempData["ErrorMessage"] = "Address not found.";
                return RedirectToAction(nameof(AddressBook));
            }

            customer.Addresses.Remove(address);
            await _unitOfWork.SaveAsync(cancellationToken);

            TempData["SuccessMessage"] = "Address has been removed from your Address Book.";
            return RedirectToAction(nameof(AddressBook));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SetDefaultAddress(Guid id, CancellationToken cancellationToken)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);
            var target = customer.Addresses.FirstOrDefault(a => a.Id == id);

            if (target == null)
            {
                TempData["ErrorMessage"] = "Address not found.";
                return RedirectToAction(nameof(AddressBook));
            }

            foreach (var a in customer.Addresses.Where(a => a.Type == target.Type))
            {
                a.IsDefault = (a.Id == target.Id);
            }

            await _unitOfWork.SaveAsync(cancellationToken);

            TempData["SuccessMessage"] = $"Default {target.Type} address updated.";
            return RedirectToAction(nameof(AddressBook));
        }

        #endregion

        #region 3. Voucher Center

        [HttpGet]
        public async Task<IActionResult> Vouchers(CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyVoucher";

            var coupons = await _mediator.SendQueryAsync<GetActiveStoreCouponsQuery, List<Coupon>>(new GetActiveStoreCouponsQuery(), cancellationToken);
            coupons ??= new List<Coupon>();

            var model = coupons.Select(c => new CustomerVoucherViewModel
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinimumSpend = c.MinimumSpend,
                ExpirationDate = c.ExpirationDate,
                IsActive = c.IsActive && c.ExpirationDate >= DateTime.UtcNow
            }).ToList();

            return View(model);
        }

        #endregion

        #region 4. My Orders, Tracking, Cancellations & Returns

        [HttpGet]
        public async Task<IActionResult> Orders(string? status, CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyOrders";
            ViewBag.CurrentStatus = status?.ToLowerInvariant();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var ordersQuery = customer.Orders.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<OrderStatus>(status, true, out var parsedStatus))
                {
                    ordersQuery = ordersQuery.Where(o => o.Status == parsedStatus);
                }
            }

            var orders = ordersQuery
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new CustomerOrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount != null ? o.TotalAmount.Amount : 0,
                    TotalItems = o.OrderItems.Sum(i => i.Quantity),
                    ItemNames = o.OrderItems.Select(i => i.ProductName).ToList()
                }).ToList();

            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> OrderDetails(Guid id, CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyOrders";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var order = customer.Orders.FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                return NotFound("Order not found or access denied.");
            }

            var shippingAddr = order.ShippingAddress != null
                ? $"{order.ShippingAddress.StreetAddress}, {order.ShippingAddress.City} - {order.ShippingAddress.PostalCode}, {order.ShippingAddress.Country}"
                : "Standard Delivery Address";

            var billingAddr = order.BillingAddress != null
                ? $"{order.BillingAddress.StreetAddress}, {order.BillingAddress.City} - {order.BillingAddress.PostalCode}, {order.BillingAddress.Country}"
                : shippingAddr;

            var model = new CustomerOrderDetailsViewModel
            {
                Id = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Status = order.Status,
                PaymentStatus = order.PaymentStatus,
                PaymentMethod = order.PaymentMethod,
                Subtotal = order.Subtotal?.Amount ?? 0,
                ShippingCost = order.ShippingCost?.Amount ?? 0,
                DiscountAmount = order.DiscountAmount?.Amount ?? 0,
                TotalAmount = order.TotalAmount?.Amount ?? 0,
                OrderNotes = order.OrderNotes,
                ShippingAddressText = shippingAddr,
                BillingAddressText = billingAddr,
                Items = order.OrderItems.Select(i => new CustomerOrderItemViewModel
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    UnitPrice = i.UnitPrice?.Amount ?? 0,
                    Quantity = i.Quantity,
                    TotalPrice = i.TotalPrice?.Amount ?? 0
                }).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Cancellations(CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyCancellation";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var cancelledOrders = customer.Orders
                .Where(o => o.Status == OrderStatus.Cancelled)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new CustomerOrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount?.Amount ?? 0,
                    TotalItems = o.OrderItems.Sum(i => i.Quantity),
                    ItemNames = o.OrderItems.Select(i => i.ProductName).ToList()
                }).ToList();

            return View(cancelledOrders);
        }

        [HttpGet]
        public async Task<IActionResult> Returns(CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyReturns";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            // Returns currently mapped to any order with return notes or returned delivery
            var returns = customer.Orders
                .Where(o => o.Status == OrderStatus.Cancelled || (o.OrderNotes != null && o.OrderNotes.Contains("Return", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new CustomerOrderSummaryViewModel
                {
                    Id = o.Id,
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    TotalAmount = o.TotalAmount?.Amount ?? 0,
                    TotalItems = o.OrderItems.Sum(i => i.Quantity),
                    ItemNames = o.OrderItems.Select(i => i.ProductName).ToList()
                }).ToList();

            return View(returns);
        }

        #endregion

        #region 5. Wishlist & Reviews

        [HttpGet]
        public async Task<IActionResult> Reviews(CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyReviews";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var reviews = customer.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new CustomerReviewViewModel
                {
                    Id = r.Id,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? "Product",
                    ProductImage = r.Product?.Images?.FirstOrDefault()?.ImageUrl,
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    IsApproved = r.IsApproved,
                    CreatedAt = r.CreatedAt
                }).ToList();

            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> Wishlist(CancellationToken cancellationToken)
        {
            ViewData["ActiveMenu"] = "MyWishlist";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var customer = await GetOrCreateCustomerAsync(user, cancellationToken);

            var items = customer.Wishlist
                .OrderByDescending(w => w.CreatedAt)
                .Select(w => new CustomerWishlistViewModel
                {
                    Id = w.Id,
                    ProductId = w.ProductId,
                    ProductName = w.Product?.Name ?? "Product",
                    ProductImage = w.Product?.Images?.FirstOrDefault()?.ImageUrl,
                    Price = (w.Product?.SalePrice ?? w.Product?.BasePrice)?.Amount ?? 0,
                    IsInStock = w.Product?.IsActive ?? false,
                    AddedAt = w.CreatedAt
                }).ToList();

            return View(items);
        }

        #endregion

        #region 6. Change Password

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewData["ActiveMenu"] = "ChangePassword";
            return View(new ChangePasswordModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordModel model)
        {
            ViewData["ActiveMenu"] = "ChangePassword";

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Your password has been successfully changed.";
                return RedirectToAction(nameof(ChangePassword));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        #endregion

        #region 7. Two-Factor Authentication

        [HttpGet]
        public async Task<IActionResult> Disable2fa()
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!await _userManager.GetTwoFactorEnabledAsync(user))
            {
                TempData["StatusMessage"] = "2FA is not currently enabled for your account.";
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            return View(new Disable2faModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Disable2fa(Disable2faModel model)
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disable2faResult.Succeeded)
            {
                throw new InvalidOperationException("Unexpected error occurred disabling 2FA.");
            }

            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
            TempData["StatusMessage"] = "2FA has been disabled. You can re-enable 2FA when you set up an authenticator app.";
            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        [HttpGet]
        public async Task<IActionResult> EnableAuthenticator()
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new EnableAuthenticatorModel();
            await LoadSharedKeyAndQrCodeUriAsync(user, model);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorModel model)
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            var verificationCode = model.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
            var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(
                user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);

            if (!is2faTokenValid)
            {
                ModelState.AddModelError(nameof(model.Code), "Verification code is invalid.");
                await LoadSharedKeyAndQrCodeUriAsync(user, model);
                return View(model);
            }

            await _userManager.SetTwoFactorEnabledAsync(user, true);
            var userId = await _userManager.GetUserIdAsync(user);
            _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

            TempData["StatusMessage"] = "Your authenticator app has been verified.";

            if (await _userManager.CountRecoveryCodesAsync(user) == 0)
            {
                var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
                TempData["RecoveryCodes"] = recoveryCodes!.ToArray();
                return RedirectToAction(nameof(ShowRecoveryCodes));
            }

            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        private async Task LoadSharedKeyAndQrCodeUriAsync(ApplicationUser user, EnableAuthenticatorModel model)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(unformattedKey))
            {
                await _userManager.ResetAuthenticatorKeyAsync(user);
                unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            }

            model.SharedKey = FormatKey(unformattedKey!);
            var email = await _userManager.GetEmailAsync(user);
            model.AuthenticatorUri = GenerateQrCodeUri(email!, unformattedKey!);
        }

        private static string FormatKey(string unformattedKey)
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

        [HttpGet]
        public async Task<IActionResult> GenerateRecoveryCodes()
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException("Cannot generate recovery codes because user does not have 2FA enabled.");
            }

            return View(new GenerateRecoveryCodesModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateRecoveryCodes(GenerateRecoveryCodesModel model)
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!isTwoFactorEnabled)
            {
                throw new InvalidOperationException("Cannot generate recovery codes because user does not have 2FA enabled.");
            }

            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
            TempData["RecoveryCodes"] = recoveryCodes!.ToArray();
            TempData["StatusMessage"] = "You have generated new recovery codes.";

            _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", user.Id);
            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        [HttpGet]
        public async Task<IActionResult> ResetAuthenticator()
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            return View(new ResetAuthenticatorModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetAuthenticator(ResetAuthenticatorModel model)
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _userManager.SetTwoFactorEnabledAsync(user, false);
            await _userManager.ResetAuthenticatorKeyAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            TempData["StatusMessage"] = "Your authenticator app key has been reset, you will need to configure your authenticator app using the new key.";

            return RedirectToAction(nameof(EnableAuthenticator));
        }

        [HttpGet]
        public IActionResult ShowRecoveryCodes()
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var recoveryCodes = TempData["RecoveryCodes"] as string[];
            if (recoveryCodes == null || recoveryCodes.Length == 0)
            {
                return RedirectToAction(nameof(TwoFactorAuthentication));
            }

            return View(new ShowRecoveryCodesModel { RecoveryCodes = recoveryCodes });
        }

        [HttpGet]
        public async Task<IActionResult> TwoFactorAuthentication()
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            var model = new TwoFactorAuthenticationModel
            {
                HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
                Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
                IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
                RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user)
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> TwoFactorAuthentication(TwoFactorAuthenticationModel model)
        {
            ViewData["ActiveMenu"] = "TwoFactor";
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await _signInManager.ForgetTwoFactorClientAsync();
            TempData["StatusMessage"] = "The current browser has been forgotten. When you log in again from this browser, you will be prompted for your 2FA code.";

            return RedirectToAction(nameof(TwoFactorAuthentication));
        }

        #endregion
    }
}