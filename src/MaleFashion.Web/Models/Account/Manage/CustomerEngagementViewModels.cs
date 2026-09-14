using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class CustomerVoucherViewModel
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumSpend { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class CustomerReviewViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerWishlistViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ProductImage { get; set; }
        public decimal Price { get; set; }
        public bool IsInStock { get; set; }
        public DateTime AddedAt { get; set; }
    }
}
