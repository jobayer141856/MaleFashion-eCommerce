using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Products.DTOs
{
    public class ProductVariantDto
    {
        public Guid? Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#000000";
        public string? Sku { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockQuantity { get; set; }
        public decimal? Price { get; set; }
    }
}
