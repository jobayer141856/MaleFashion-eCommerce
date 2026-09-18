using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Checkout
{
    public class OrderConfirmationViewModel
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public string? TransactionId { get; set; }

        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;

        public string ShippingAddressText { get; set; } = string.Empty;
        public string BillingAddressText { get; set; } = string.Empty;
        public string? OrderNotes { get; set; }

        public List<OrderConfirmationItemViewModel> Items { get; set; } = new();
    }

    public class OrderConfirmationItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

