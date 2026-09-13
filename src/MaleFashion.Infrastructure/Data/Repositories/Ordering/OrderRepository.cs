using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Features.Dashboard.DTOs;
using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Entities.Reviews;
using MaleFashion.Domain.Entities.Support;
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

        public async Task<AdminDashboardDto> GetDashboardOverviewAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var startOfToday = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var thirtyDaysAgo = now.AddDays(-30);
            var sixtyDaysAgo = now.AddDays(-60);

            // 1. Orders & Financial KPIs
            var totalOrders = await _context.Set<Order>().CountAsync(cancellationToken);
            var todayOrdersCount = await _context.Set<Order>().CountAsync(o => o.OrderDate >= startOfToday, cancellationToken);
            var pendingOrders = await _context.Set<Order>().CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
            var processingOrders = await _context.Set<Order>().CountAsync(o => o.Status == OrderStatus.Processing, cancellationToken);
            var shippedOrders = await _context.Set<Order>().CountAsync(o => o.Status == OrderStatus.Shipped, cancellationToken);
            var deliveredOrders = await _context.Set<Order>().CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);
            var cancelledOrders = await _context.Set<Order>().CountAsync(o => o.Status == OrderStatus.Cancelled, cancellationToken);

            var validOrders = _context.Set<Order>().AsNoTracking().Where(o => o.Status != OrderStatus.Cancelled);
            var totalRevenue = totalOrders > 0 ? (await validOrders.SumAsync(o => (decimal?)o.TotalAmount.Amount, cancellationToken) ?? 0) : 0;
            var todayRevenue = await validOrders.Where(o => o.OrderDate >= startOfToday).SumAsync(o => (decimal?)o.TotalAmount.Amount, cancellationToken) ?? 0;
            var monthRevenue = await validOrders.Where(o => o.OrderDate >= startOfMonth).SumAsync(o => (decimal?)o.TotalAmount.Amount, cancellationToken) ?? 0;

            var validOrdersCount = await validOrders.CountAsync(cancellationToken);
            var aov = validOrdersCount > 0 ? Math.Round(totalRevenue / validOrdersCount, 2) : 0;

            var revenueLast30 = await validOrders.Where(o => o.OrderDate >= thirtyDaysAgo).SumAsync(o => (decimal?)o.TotalAmount.Amount, cancellationToken) ?? 0;
            var revenuePrev30 = await validOrders.Where(o => o.OrderDate >= sixtyDaysAgo && o.OrderDate < thirtyDaysAgo).SumAsync(o => (decimal?)o.TotalAmount.Amount, cancellationToken) ?? 0;
            var growthPct = revenuePrev30 > 0 ? Math.Round(((revenueLast30 - revenuePrev30) / revenuePrev30) * 100, 1) : (revenueLast30 > 0 ? 100m : 0m);

            // 2. Customers & Carts
            var totalCustomers = await _context.Set<Customer>().CountAsync(cancellationToken);
            var newCustomersThisMonth = await _context.Set<Customer>().CountAsync(c => c.CreatedAt >= startOfMonth, cancellationToken);
            var activeCarts = await _context.Set<Cart>().CountAsync(c => c.Items.Any(), cancellationToken);
            var cartPotentialRevenue = await _context.Set<CartItem>().SumAsync(ci => (decimal?)(ci.Quantity * ci.UnitPrice.Amount), cancellationToken) ?? 0;

            // 3. Inventory & Promotions
            var totalProducts = await _context.Set<Product>().CountAsync(cancellationToken);
            var totalStockUnits = await _context.Set<ProductVariant>().SumAsync(v => (int?)v.StockQuantity, cancellationToken) ?? 0;
            var lowStockCount = await _context.Set<Product>().CountAsync(p => p.Variants.Any(v => v.StockQuantity <= v.lowStockQuantity && v.StockQuantity > 0), cancellationToken);
            var outOfStockCount = await _context.Set<Product>().CountAsync(p => !p.Variants.Any() || p.Variants.All(v => v.StockQuantity == 0), cancellationToken);
            var activeDealsCount = await _context.Set<DealCountdown>().CountAsync(d => d.IsActive && (!d.StartDate.HasValue || d.StartDate <= now) && d.EndDate >= now, cancellationToken);
            var activeCouponsCount = await _context.Set<Coupon>().CountAsync(c => c.IsActive && (!c.StartDate.HasValue || c.StartDate <= now) && c.ExpirationDate >= now, cancellationToken);

            // 4. Moderation & Inquiries
            var pendingReviewsCount = await _context.Set<ProductReview>().CountAsync(r => !r.IsApproved, cancellationToken);
            var unreadInquiriesCount = await _context.Set<ContactInquiry>().CountAsync(ci => !ci.IsResolved, cancellationToken);
            var averageRating = await _context.Set<ProductReview>().Where(r => r.IsApproved).AverageAsync(r => (double?)r.Rating, cancellationToken) ?? 5.0;

            // 5. 7-Day Revenue & Orders Trend
            var revenueChartLabels = new List<string>();
            var revenueChartData = new List<decimal>();
            var ordersChartData = new List<int>();

            var sevenDaysAgo = now.Date.AddDays(-6);
            var ordersLast7Days = await _context.Set<Order>()
                .AsNoTracking()
                .Where(o => o.OrderDate >= sevenDaysAgo && o.Status != OrderStatus.Cancelled)
                .Select(o => new { o.OrderDate, Amount = o.TotalAmount.Amount })
                .ToListAsync(cancellationToken);

            for (int i = 0; i < 7; i++)
            {
                var dayDate = sevenDaysAgo.AddDays(i);
                var nextDay = dayDate.AddDays(1);
                var dayLabel = dayDate.ToString("MMM dd");

                var dayOrders = ordersLast7Days.Where(o => o.OrderDate >= dayDate && o.OrderDate < nextDay).ToList();
                decimal dayRevenue = dayOrders.Sum(o => o.Amount);
                int dayCount = dayOrders.Count;

                revenueChartLabels.Add(dayLabel);
                revenueChartData.Add(dayRevenue);
                ordersChartData.Add(dayCount);
            }

            // 6. Category Breakdown & Top Products from OrderItems
            var orderItemsFlat = await _context.Set<OrderItem>()
                .AsNoTracking()
                .Where(i => i.Order != null && i.Order.Status != OrderStatus.Cancelled)
                .Select(i => new
                {
                    i.ProductId,
                    i.ProductName,
                    i.Quantity,
                    Amount = i.TotalPrice.Amount,
                    CategoryName = i.Product != null && i.Product.Category != null ? i.Product.Category.Name : "General"
                })
                .ToListAsync(cancellationToken);

            var categorySalesList = orderItemsFlat
                .GroupBy(i => i.CategoryName)
                .Select(g => new CategorySalesDto
                {
                    CategoryName = g.Key,
                    TotalSales = g.Sum(x => x.Amount),
                    ItemsCount = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(c => c.TotalSales)
                .Take(5)
                .ToList();

            decimal totalCatSales = categorySalesList.Sum(c => c.TotalSales);
            foreach (var item in categorySalesList)
            {
                item.Percentage = totalCatSales > 0 ? Math.Round((item.TotalSales / totalCatSales) * 100, 1) : 0;
            }

            // Top Products
            var topProductsGrouped = orderItemsFlat
                .GroupBy(i => new { i.ProductId, i.ProductName, i.CategoryName })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    g.Key.CategoryName,
                    UnitsSold = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.Amount)
                })
                .OrderByDescending(p => p.UnitsSold)
                .Take(5)
                .ToList();

            var topProductIds = topProductsGrouped.Select(p => p.ProductId).ToList();
            var productEntities = await _context.Set<Product>()
                .AsNoTracking()
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .Where(p => topProductIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            var topSellingProducts = topProductsGrouped.Select(p =>
            {
                var entity = productEntities.FirstOrDefault(e => e.Id == p.ProductId);
                var primaryImg = entity?.Images.FirstOrDefault(img => img.IsPrimary)?.ImageUrl 
                              ?? entity?.Images.FirstOrDefault()?.ImageUrl;
                var stock = entity?.Variants.Sum(v => v.StockQuantity) ?? 0;

                return new DashboardTopProductDto
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Sku = entity?.Sku ?? "N/A",
                    CategoryName = p.CategoryName,
                    ImageUrl = primaryImg,
                    Price = entity?.BasePrice != null ? entity.BasePrice.Amount : 0,
                    UnitsSold = p.UnitsSold,
                    TotalRevenue = p.TotalRevenue,
                    CurrentStock = stock
                };
            }).ToList();

            // 7. Recent 6 Orders
            var recentOrdersEntities = await _context.Set<Order>()
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .Take(6)
                .ToListAsync(cancellationToken);

            var recentOrders = recentOrdersEntities.Select(o => new DashboardRecentOrderDto
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                CustomerName = o.Customer != null ? $"{o.Customer.FirstName} {o.Customer.LastName}".Trim() : "Guest Customer",
                CustomerEmail = o.Customer?.Email ?? "N/A",
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount.Amount,
                Currency = o.TotalAmount.Currency,
                Status = o.Status.ToString(),
                PaymentStatus = o.PaymentStatus.ToString(),
                PaymentMethod = o.PaymentMethod.ToString(),
                ItemsCount = o.OrderItems.Sum(i => i.Quantity)
            }).ToList();

            return new AdminDashboardDto
            {
                TotalRevenue = totalRevenue,
                TodayRevenue = todayRevenue,
                MonthRevenue = monthRevenue,
                AverageOrderValue = aov,
                RevenueGrowthPercentage = growthPct,
                TotalOrders = totalOrders,
                TodayOrdersCount = todayOrdersCount,
                PendingOrders = pendingOrders,
                ProcessingOrders = processingOrders,
                ShippedOrders = shippedOrders,
                DeliveredOrders = deliveredOrders,
                CancelledOrders = cancelledOrders,
                TotalCustomers = totalCustomers,
                NewCustomersThisMonth = newCustomersThisMonth,
                ActiveCartsCount = activeCarts,
                CartPotentialRevenue = cartPotentialRevenue,
                TotalProducts = totalProducts,
                TotalStockUnits = totalStockUnits,
                LowStockAlertCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                ActiveDealsCount = activeDealsCount,
                ActiveCouponsCount = activeCouponsCount,
                PendingReviewsCount = pendingReviewsCount,
                UnreadInquiriesCount = unreadInquiriesCount,
                AverageRating = Math.Round(averageRating, 1),
                RevenueChartLabels = revenueChartLabels,
                RevenueChartData = revenueChartData,
                OrdersChartData = ordersChartData,
                CategorySales = categorySalesList,
                RecentOrders = recentOrders,
                TopSellingProducts = topSellingProducts
            };
        }
    }
}

