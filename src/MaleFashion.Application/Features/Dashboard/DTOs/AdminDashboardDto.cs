using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Dashboard.DTOs
{
    public class AdminDashboardDto
    {
        // Financial & Velocity KPIs
        public decimal TotalRevenue { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal RevenueGrowthPercentage { get; set; }

        // Orders Pipeline
        public int TotalOrders { get; set; }
        public int TodayOrdersCount { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public int UnfulfilledOrdersCount => PendingOrders + ProcessingOrders;

        // Customers & Carts
        public int TotalCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public int ActiveCartsCount { get; set; }
        public decimal CartPotentialRevenue { get; set; }

        // Inventory Health & Promotions
        public int TotalProducts { get; set; }
        public int TotalStockUnits { get; set; }
        public int LowStockAlertCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int ActiveDealsCount { get; set; }
        public int ActiveCouponsCount { get; set; }

        // Feedback & Inquiries
        public int PendingReviewsCount { get; set; }
        public int UnreadInquiriesCount { get; set; }
        public double AverageRating { get; set; }

        // Chart Data Series (Last 7 Days)
        public List<string> RevenueChartLabels { get; set; } = new List<string>();
        public List<decimal> RevenueChartData { get; set; } = new List<decimal>();
        public List<int> OrdersChartData { get; set; } = new List<int>();
        public List<CategorySalesDto> CategorySales { get; set; } = new List<CategorySalesDto>();

        // Operational Feeds
        public List<DashboardRecentOrderDto> RecentOrders { get; set; } = new List<DashboardRecentOrderDto>();
        public List<DashboardTopProductDto> TopSellingProducts { get; set; } = new List<DashboardTopProductDto>();
    }
}

