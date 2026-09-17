using System;

namespace MaleFashion.Application.Features.Wishlists.DTOs
{
    public class CustomerWishlistItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; } = "BDT";
        public bool IsInStock { get; set; }
        public int StockQuantity { get; set; }
        public DateTime AddedAt { get; set; }
    }
}

