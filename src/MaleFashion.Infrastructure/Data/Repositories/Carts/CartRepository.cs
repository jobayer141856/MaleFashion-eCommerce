using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Domain.Entities.Carts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Carts
{
    public class CartRepository : Repository<Cart, Guid>, ICartRepository
    {
        private readonly ApplicationDbContext _context;

        public CartRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Cart> Items, int Total, int TotalDisplay)> GetPagedActiveCarts(GetAllActiveCartsByPagingQuery query, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var twoHoursAgo = now.AddHours(-2);
            var oneDayAgo = now.AddHours(-24);

            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "CreatedAt desc" : query.SortText;

            return await GetDynamicAsync(
                x => x.Items.Any() &&
                     (string.IsNullOrWhiteSpace(query.SearchText) ||
                      (x.Customer != null && (x.Customer.FirstName.Contains(query.SearchText) || x.Customer.LastName.Contains(query.SearchText) || x.Customer.Email.Contains(query.SearchText) || x.Customer.PhoneNumber.Contains(query.SearchText))) ||
                      x.SessionId.Contains(query.SearchText) ||
                      (x.AppliedCouponCode != null && x.AppliedCouponCode.Contains(query.SearchText)) ||
                      x.Items.Any(i => i.ProductName.Contains(query.SearchText))) &&
                     (string.IsNullOrWhiteSpace(query.CartFilter) || query.CartFilter == "all" ||
                      (query.CartFilter == "active" && (x.UpdatedAt ?? x.CreatedAt) >= twoHoursAgo) ||
                      (query.CartFilter == "idle" && (x.UpdatedAt ?? x.CreatedAt) < twoHoursAgo && (x.UpdatedAt ?? x.CreatedAt) >= oneDayAgo) ||
                      (query.CartFilter == "abandoned" && (x.UpdatedAt ?? x.CreatedAt) < oneDayAgo) ||
                      (query.CartFilter == "registered" && x.CustomerId != null) ||
                      (query.CartFilter == "guest" && x.CustomerId == null)),
                sortText,
                q => q.Include(c => c.Customer).Include(c => c.Items).ThenInclude(i => i.Product).ThenInclude(p => p.Images),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<Cart?> GetCartWithDetailsById(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<Cart>()
                .Include(c => c.Customer)
                    .ThenInclude(cust => cust.Addresses)
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<Cart?> GetActiveCartByCustomerOrSessionAsync(Guid? customerId, string? sessionId, CancellationToken cancellationToken)
        {
            if (customerId.HasValue)
            {
                var customerCart = await _context.Set<Cart>()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Images)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.ProductVariant)
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .FirstOrDefaultAsync(c => c.CustomerId == customerId.Value, cancellationToken);

                if (customerCart != null)
                    return customerCart;
            }

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                return await _context.Set<Cart>()
                    .Include(c => c.Items)
                        .ThenInclude(i => i.Product)
                            .ThenInclude(p => p.Images)
                    .Include(c => c.Items)
                        .ThenInclude(i => i.ProductVariant)
                    .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId, cancellationToken);
            }

            return null;
        }

        public async Task<ActiveCartStatsDto> GetActiveCartStatsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var twoHoursAgo = now.AddHours(-2);
            var oneDayAgo = now.AddHours(-24);

            var activeCartsQuery = _context.Set<Cart>().Where(c => c.Items.Any());

            var total = await activeCartsQuery.CountAsync(cancellationToken);
            var activeRecent = await activeCartsQuery.CountAsync(c => (c.UpdatedAt ?? c.CreatedAt) >= twoHoursAgo, cancellationToken);
            var idle = await activeCartsQuery.CountAsync(c => (c.UpdatedAt ?? c.CreatedAt) < twoHoursAgo && (c.UpdatedAt ?? c.CreatedAt) >= oneDayAgo, cancellationToken);
            var abandoned = await activeCartsQuery.CountAsync(c => (c.UpdatedAt ?? c.CreatedAt) < oneDayAgo, cancellationToken);
            var registered = await activeCartsQuery.CountAsync(c => c.CustomerId != null, cancellationToken);
            var guest = total - registered;

            // Potential revenue calculation
            var itemsQuery = _context.Set<CartItem>().Where(i => i.Cart != null && i.Cart.Items.Any());

            decimal totalRevenue = 0;
            decimal registeredRevenue = 0;

            if (total > 0)
            {
                totalRevenue = await itemsQuery.SumAsync(i => i.Quantity * i.UnitPrice.Amount, cancellationToken);
                registeredRevenue = await itemsQuery.Where(i => i.Cart!.CustomerId != null).SumAsync(i => i.Quantity * i.UnitPrice.Amount, cancellationToken);
            }

            var guestRevenue = totalRevenue - registeredRevenue;

            return new ActiveCartStatsDto
            {
                TotalActiveCarts = total,
                PotentialRevenue = totalRevenue,
                RegisteredCartsCount = registered,
                GuestCartsCount = guest,
                ActiveRecentCount = activeRecent,
                IdleCount = idle,
                AbandonedCount = abandoned,
                RegisteredRevenue = registeredRevenue,
                GuestRevenue = guestRevenue
            };
        }

        public async Task<int> ClearStaleCartsAsync(int olderThanDays, CancellationToken cancellationToken)
        {
            var threshold = DateTime.UtcNow.AddDays(-olderThanDays);
            var staleCarts = await _context.Set<Cart>()
                .Where(c => (c.UpdatedAt ?? c.CreatedAt) < threshold)
                .ToListAsync(cancellationToken);

            if (!staleCarts.Any())
                return 0;

            _context.Set<Cart>().RemoveRange(staleCarts);
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

