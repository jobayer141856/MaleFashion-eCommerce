using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class CustomerOrderSummaryViewModel
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalItems { get; set; }
        public List<string> ItemNames { get; set; } = new();
    }

    public class CustomerOrderItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public string? ImageUrl { get; set; }
    }

    public class CustomerOrderDetailsViewModel
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? OrderNotes { get; set; }
        public string ShippingAddressText { get; set; } = string.Empty;
        public string BillingAddressText { get; set; } = string.Empty;
        public List<CustomerOrderItemViewModel> Items { get; set; } = new();
    }
}

