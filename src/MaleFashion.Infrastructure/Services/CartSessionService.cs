using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Services
{
    public class CartSessionService : ICartSessionService
    {
        private const string CartCookieName = "MF_CartSessionId";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartSessionService(
            IHttpContextAccessor httpContextAccessor,
            IApplicationUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<(Guid? CustomerId, string SessionId)> GetCartIdentityAsync(CancellationToken cancellationToken = default)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            string sessionId = string.Empty;

            if (httpContext != null)
            {
                // 1. Resolve SessionId from cookie or generate new
                sessionId = httpContext.Request.Cookies[CartCookieName] ?? string.Empty;
                if (string.IsNullOrWhiteSpace(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString("N");
                    httpContext.Response.Cookies.Append(CartCookieName, sessionId, new CookieOptions
                    {
                        HttpOnly = true,
                        IsEssential = true,
                        SameSite = SameSiteMode.Lax,
                        Expires = DateTimeOffset.UtcNow.AddDays(30)
                    });
                }
            }
            else
            {
                sessionId = Guid.NewGuid().ToString("N");
            }

            // 2. Resolve CustomerId if user is authenticated
            Guid? customerId = null;
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = _userManager.GetUserId(httpContext.User);
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var customer = await _unitOfWork.CustomerRepository.GetCustomerByUserIdAsync(userId, cancellationToken);
                    if (customer != null)
                    {
                        customerId = customer.Id;

                        // Auto-merge guest cart into authenticated customer cart if session cart exists
                        if (!string.IsNullOrWhiteSpace(sessionId))
                        {
                            await MergeGuestCartAsync(customer.Id, sessionId, cancellationToken);
                        }
                    }
                }
            }

            return (customerId, sessionId);
        }

        private async Task MergeGuestCartAsync(Guid customerId, string sessionId, CancellationToken cancellationToken)
        {
            // 1. Find the guest cart by SessionId
            var guestCart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(null, sessionId, cancellationToken);
            if (guestCart == null || guestCart.CustomerId == customerId)
                return;

            // 2. Find the customer's existing account cart
            var customerCart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(customerId, null, cancellationToken);

            if (!guestCart.Items.Any())
            {
                if (customerCart == null)
                {
                    guestCart.CustomerId = customerId;
                    guestCart.UpdatedAt = DateTime.UtcNow;
                    await _unitOfWork.SaveAsync(cancellationToken);
                }
                return;
            }

            if (customerCart == null)
            {
                // Customer has no existing cart; simply claim the guest cart
                guestCart.CustomerId = customerId;
                guestCart.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Merge guest items into customer's existing cart
                foreach (var guestItem in guestCart.Items.ToList())
                {
                    var existingItem = customerCart.Items.FirstOrDefault(i =>
                        i.ProductId == guestItem.ProductId &&
                        i.ProductVariantId == guestItem.ProductVariantId);

                    if (existingItem != null)
                    {
                        existingItem.Quantity += guestItem.Quantity;
                        existingItem.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        customerCart.Items.Add(new CartItem
                        {
                            CartId = customerCart.Id,
                            ProductId = guestItem.ProductId,
                            ProductVariantId = guestItem.ProductVariantId,
                            ProductName = guestItem.ProductName,
                            Quantity = guestItem.Quantity,
                            UnitPrice = guestItem.UnitPrice,
                            ImageUrl = guestItem.ImageUrl,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                // If guest cart had an applied coupon and customer cart doesn't, carry it over
                if (string.IsNullOrWhiteSpace(customerCart.AppliedCouponCode) && !string.IsNullOrWhiteSpace(guestCart.AppliedCouponCode))
                {
                    customerCart.AppliedCouponCode = guestCart.AppliedCouponCode;
                }

                customerCart.UpdatedAt = DateTime.UtcNow;

                // Remove the orphaned guest cart
                _unitOfWork.CartRepository.Remove(guestCart);
            }

            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

