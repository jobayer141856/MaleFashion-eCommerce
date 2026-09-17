using System;

namespace MaleFashion.Application.Features.Carts.DTOs
{
    public class CustomerCartItemDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Size { get; set; }
        public string? ColorName { get; set; }
        public string ImageUrl { get; set; } = "/malefashion/img/product/product-default.jpg";
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal => UnitPrice * Quantity;
    }
}

