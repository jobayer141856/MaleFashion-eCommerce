using MaleFashion.Application.Contracts;
using MaleFashion.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Services
{
    public interface ICartSessionService
    {
        Task<(Guid? CustomerId, string SessionId)> GetCartIdentityAsync(HttpContext context, CancellationToken cancellationToken = default);
    }

    public class CartSessionService : ICartSessionService
    {
        private const string CartCookieName = "MF_CartSessionId";
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartSessionService(
            IApplicationUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<(Guid? CustomerId, string SessionId)> GetCartIdentityAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            // 1. Resolve SessionId from cookie or create new
            string? sessionId = context.Request.Cookies[CartCookieName];
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = Guid.NewGuid().ToString("N");
                context.Response.Cookies.Append(CartCookieName, sessionId, new CookieOptions
                {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
            }

            // 2. Resolve CustomerId if user is authenticated
            Guid? customerId = null;
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var userIdStr = _userManager.GetUserId(context.User);
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

