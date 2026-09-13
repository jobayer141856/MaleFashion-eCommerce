using System;

namespace MaleFashion.Application.Features.Catalogs.Products.DTOs
{
    public class ProductActiveDealDto
    {
        public Guid DealId { get; set; }
        public Guid ProductId { get; set; }
        public string DealTitle { get; set; } = string.Empty;
        public decimal SalePrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsLive { get; set; }
    }
}

