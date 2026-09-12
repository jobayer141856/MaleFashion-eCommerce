using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Areas.Admin.Models.Carts
{
    public class ActiveCartViewModel
    {
        public Guid Id { get; set; }
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string? CustomerAddress { get; set; }
        public bool IsRegistered { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string? AppliedCouponCode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public decimal TotalValue { get; set; }
        public int TotalItemsCount { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public List<CartItemViewModel> Items { get; set; } = new();
    }

    public class CartItemViewModel
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? VariantDetails { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}

