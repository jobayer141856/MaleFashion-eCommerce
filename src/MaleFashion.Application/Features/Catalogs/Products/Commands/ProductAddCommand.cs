using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductVariantDto
    {
        public string Size { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#000000";
        public string? Sku { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockQuantity { get; set; }
        public decimal? Price { get; set; }
    }

    public class ProductAddCommand : ICommand<Product>
    {
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AdditionalInformation { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public Guid CategoryId { get; set; }
        public Guid BrandId { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsNewArrival { get; set; }
        public bool IsHotSale { get; set; }

        public string? PrimaryImageName { get; set; }
        public List<string> GalleryImageNames { get; set; } = new();
        public List<ProductVariantDto> Variants { get; set; } = new();
    }
}

