using System;
using System.Collections.Generic;
using System.Linq;

namespace MaleFashion.Application.Features.Catalogs.Products.DTOs
{
    public class ShopProductItemDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public decimal EffectivePrice { get; set; }
        public double Rating { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsHotSale { get; set; }
        public string? CategoryName { get; set; }
        public string? BrandName { get; set; }
        public string ImageUrl { get; set; } = "/malefashion/img/product/product-default.jpg";
        public string? AvailableColorsHex { get; set; }
        public DateTime CreatedAt { get; set; }

        public bool IsOnSale => SalePrice.HasValue && SalePrice.Value < BasePrice;

        public List<string> ColorList => string.IsNullOrWhiteSpace(AvailableColorsHex)
            ? new List<string>()
            : AvailableColorsHex.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }
}

