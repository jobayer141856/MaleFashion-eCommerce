using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Services
{
    public class WishlistSessionService : IWishlistSessionService
    {
        private const string WishlistCookieName = "MF_GuestWishlist";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistSessionService(
            IHttpContextAccessor httpContextAccessor,
            IApplicationUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<(Guid? CustomerId, List<Guid> GuestProductIds)> GetWishlistIdentityAsync(CancellationToken cancellationToken = default)
        {
            Guid? customerId = null;
            var httpContext = _httpContextAccessor.HttpContext;

            // 1. Resolve CustomerId if authenticated
            if (httpContext?.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = _userManager.GetUserId(httpContext.User);
                if (Guid.TryParse(userIdStr, out var userId))
                {
                    var customer = await _unitOfWork.CustomerRepository.GetCustomerByUserIdAsync(userId, cancellationToken);
                    if (customer != null)
                    {
                        customerId = customer.Id;

                        // Auto-sync guest wishlist items if any exist in cookie
                        var guestIds = GetGuestWishlist();
                        if (guestIds.Any())
                        {
                            var existingWishlistProductIds = customer.Wishlist.Select(w => w.ProductId).ToHashSet();
                            bool changed = false;

                            foreach (var gId in guestIds)
                            {
                                if (!existingWishlistProductIds.Contains(gId))
                                {
                                    customer.Wishlist.Add(new WishlistItem
                                    {
                                        Id = Guid.NewGuid(),
                                        CustomerId = customer.Id,
                                        ProductId = gId,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                    changed = true;
                                }
                            }

                            if (changed)
                            {
                                await _unitOfWork.SaveAsync(cancellationToken);
                            }

                            // Clear guest cookie after syncing
                            httpContext.Response.Cookies.Delete(WishlistCookieName);
                        }
                    }
                }
            }

            var guestProductIds = customerId.HasValue ? new List<Guid>() : GetGuestWishlist();
            return (customerId, guestProductIds);
        }

        public List<Guid> GetGuestWishlist()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return new List<Guid>();

            var cookie = httpContext.Request.Cookies[WishlistCookieName];
            if (string.IsNullOrWhiteSpace(cookie))
                return new List<Guid>();

            try
            {
                var list = JsonSerializer.Deserialize<List<Guid>>(cookie);
                return list ?? new List<Guid>();
            }
            catch
            {
                return new List<Guid>();
            }
        }

        public void SaveGuestWishlist(List<Guid> productIds)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return;

            var json = JsonSerializer.Serialize(productIds);
            httpContext.Response.Cookies.Append(WishlistCookieName, json, new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30)
            });
        }

        public bool ToggleGuestWishlist(Guid productId)
        {
            var list = GetGuestWishlist();
            bool isAdded;

            if (list.Contains(productId))
            {
                list.Remove(productId);
                isAdded = false;
            }
            else
            {
                list.Add(productId);
                isAdded = true;
            }

            SaveGuestWishlist(list);
            return isAdded;
        }

        public void RemoveFromGuestWishlist(Guid productId)
        {
            var list = GetGuestWishlist();
            if (list.Contains(productId))
            {
                list.Remove(productId);
                SaveGuestWishlist(list);
            }
        }
    }
}

