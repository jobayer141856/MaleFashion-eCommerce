using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Home.DTOs
{
    public class HomeProductDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public decimal BasePrice { get; set; }
        public decimal? SalePrice { get; set; }
        public string Currency { get; set; } = "BDT";
        public double Rating { get; set; }
        public bool IsNewArrival { get; set; }
        public bool IsHotSale { get; set; }
        public string PrimaryImageUrl { get; set; } = string.Empty;
        public List<string> AvailableColors { get; set; } = new();

        public bool HasDiscount => SalePrice.HasValue && SalePrice.Value < BasePrice;

        public decimal DisplayPrice => HasDiscount ? SalePrice!.Value : BasePrice;

        public string MixItUpClasses
        {
            get
            {
                var classes = new List<string> { "mix" };
                if (IsNewArrival) classes.Add("new-arrivals");
                if (IsHotSale || HasDiscount) classes.Add("hot-sales");
                return string.Join(" ", classes);
            }
        }
    }
}

