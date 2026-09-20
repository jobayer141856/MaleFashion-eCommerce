using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Shop
{
    public class ShopIndexModel
    {
        public IList<ShopProductItemDto> Products { get; set; } = new List<ShopProductItemDto>();
        public int Total { get; set; }
        public int TotalDisplay { get; set; }

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string OrderBy { get; set; } = "Newest";
        public string? SearchText { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? TagName { get; set; }

        public List<CategoryFilterItem> Categories { get; set; } = new();
        public List<BrandFilterItem> Brands { get; set; } = new();
        public List<string> Sizes { get; set; } = new() { "xs", "s", "m", "l", "xl", "2xl", "xxl", "3xl", "4xl" };
        public List<string> Tags { get; set; } = new() { "Product", "Bags", "Shoes", "Fashion", "Clothing", "Hats", "Accessories" };

        public int TotalPages => (int)Math.Ceiling((double)TotalDisplay / (PageSize > 0 ? PageSize : 12));
        public int StartRecord => TotalDisplay == 0 ? 0 : ((PageIndex - 1) * PageSize) + 1;
        public int EndRecord => Math.Min(PageIndex * PageSize, TotalDisplay);
    }

    public class CategoryFilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }

    public class BrandFilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

