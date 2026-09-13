using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Ordering
{
    public class OrderRepository : Repository<Order, Guid>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Order> Items, int Total, int TotalDisplay)> GetPagedOrders(GetAllOrdersByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "OrderDate desc" : query.SortText;
            if (sortText.Contains("TotalAmount") && !sortText.Contains("TotalAmount.Amount"))
            {
                sortText = sortText.Replace("TotalAmount", "TotalAmount.Amount");
            }

            return await GetDynamicAsync(
                o => (string.IsNullOrWhiteSpace(query.SearchText) ||
                      o.OrderNumber.Contains(query.SearchText) ||
                      (o.Customer != null && (o.Customer.FirstName.Contains(query.SearchText) || o.Customer.LastName.Contains(query.SearchText) || o.Customer.Email.Contains(query.SearchText) || o.Customer.PhoneNumber.Contains(query.SearchText))) ||
                      (o.TransactionId != null && o.TransactionId.Contains(query.SearchText)) ||
                      o.OrderItems.Any(i => i.ProductName.Contains(query.SearchText))) &&
                     (!query.StatusFilter.HasValue || o.Status == query.StatusFilter.Value) &&
                     (!query.PaymentStatusFilter.HasValue || o.PaymentStatus == query.PaymentStatusFilter.Value) &&
                     (!query.PaymentMethodFilter.HasValue || o.PaymentMethod == query.PaymentMethodFilter.Value),
                sortText,
                q => q.Include(o => o.Customer).Include(o => o.OrderItems),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<Order?> GetOrderWithDetailsById(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<Order>()
                .Include(o => o.Customer)
                    .ThenInclude(c => c.Addresses)
                .Include(o => o.Coupon)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Images)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        }

        public async Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken)
        {
            var total = await GetCountAsync(null, cancellationToken);
            var pending = await GetCountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
            var processing = await GetCountAsync(o => o.Status == OrderStatus.Processing, cancellationToken);
            var shipped = await GetCountAsync(o => o.Status == OrderStatus.Shipped, cancellationToken);
            var delivered = await GetCountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
            var cancelled = await GetCountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken);

            var paidCount = await GetCountAsync(o => o.PaymentStatus == PaymentStatus.Paid, cancellationToken);
            var pendingPaymentCount = await GetCountAsync(o => o.PaymentStatus == PaymentStatus.Pending, cancellationToken);

            decimal totalSales = 0;
            decimal cashSales = 0;
            decimal stripeSales = 0;

            if (total > 0)
            {
                var validOrders = _context.Set<Order>().Where(o => o.Status != OrderStatus.Cancelled);
                totalSales = await validOrders.SumAsync(o => o.TotalAmount.Amount, cancellationToken);
                cashSales = await validOrders.Where(o => o.PaymentMethod == PaymentMethod.Cash).SumAsync(o => o.TotalAmount.Amount, cancellationToken);
                stripeSales = await validOrders.Where(o => o.PaymentMethod == PaymentMethod.Stripe).SumAsync(o => o.TotalAmount.Amount, cancellationToken);
            }

            return new OrderStatsDto
            {
                TotalOrders = total,
                PendingOrders = pending,
                ProcessingOrders = processing,
                ShippedOrders = shipped,
                DeliveredOrders = delivered,
                CancelledOrders = cancelled,
                PaidOrdersCount = paidCount,
                PendingPaymentOrdersCount = pendingPaymentCount,
                TotalSales = totalSales,
                CashSales = cashSales,
                StripeSales = stripeSales
            };
        }
    }
}

