using System;

namespace MaleFashion.Application.Features.Dashboard.DTOs
{
    public class CategorySalesDto
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int ItemsCount { get; set; }
        public decimal Percentage { get; set; }
    }
}

