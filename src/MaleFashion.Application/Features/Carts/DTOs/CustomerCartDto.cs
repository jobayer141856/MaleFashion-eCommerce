using System;
using System.Collections.Generic;
using System.Linq;

namespace MaleFashion.Application.Features.Carts.DTOs
{
    public class CustomerCartDto
    {
        public Guid? CartId { get; set; }
        public List<CustomerCartItemDto> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.LineTotal);
        public decimal DiscountAmount { get; set; }
        public decimal Total => Math.Max(0, Subtotal - DiscountAmount);
        public int TotalItemCount => Items.Sum(i => i.Quantity);
        public string? AppliedCouponCode { get; set; }
        public string Currency { get; set; } = "BDT";
    }
}

