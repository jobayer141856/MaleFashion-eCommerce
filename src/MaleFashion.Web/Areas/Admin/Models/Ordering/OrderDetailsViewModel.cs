using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Areas.Admin.Models.Ordering
{
    public class OrderDetailsViewModel
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string? TransactionId { get; set; }
        public string? OrderNotes { get; set; }

        // Customer Info
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public bool IsGuest { get; set; }

        // Addresses
        public string ShippingAddressFormatted { get; set; } = string.Empty;
        public string BillingAddressFormatted { get; set; } = string.Empty;

        // Financials
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Currency { get; set; } = "BDT";
        public string? CouponCode { get; set; }

        // Items
        public List<OrderItemViewModel> Items { get; set; } = new();

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OrderItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantInfo { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "BDT";
    }
}

