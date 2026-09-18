using System;

namespace MaleFashion.Application.Features.Ordering.DTOs
{
    public class OrderStatsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int ShippedOrders { get; set; }
        public int DeliveredOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalSales { get; set; }
        public decimal CashSales { get; set; }
        public decimal SSLCommerzSales { get; set; }
        public decimal StripeSales { get; set; }
        public int PaidOrdersCount { get; set; }
        public int PendingPaymentOrdersCount { get; set; }
    }
}

