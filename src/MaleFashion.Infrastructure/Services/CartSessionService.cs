using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
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
                    }
                }
            }

            return (customerId, sessionId);
        }
    }
}

