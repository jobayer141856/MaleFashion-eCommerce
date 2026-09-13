using System;

namespace MaleFashion.Application.Features.Dashboard.DTOs
{
    public class DashboardTopProductDto
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int CurrentStock { get; set; }
    }
}

