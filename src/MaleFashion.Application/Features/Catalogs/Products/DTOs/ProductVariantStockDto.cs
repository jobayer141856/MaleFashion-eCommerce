using System;

namespace MaleFashion.Application.Features.Catalogs.Products.DTOs
{
    public class ProductVariantStockDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Size { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public int LowStockQuantity { get; set; }
        public decimal? VariantPrice { get; set; }
        public string HealthStatus { get; set; } = "Healthy";
    }
}

