using System;

namespace MaleFashion.Application.Features.Catalogs.Products.DTOs
{
    public class ProductCatalogStatsDto
    {
        public int TotalProducts { get; set; }
        public int TotalStockUnits { get; set; }
        public int LowStockAlertCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int ActiveDealsCount { get; set; }
    }
}

