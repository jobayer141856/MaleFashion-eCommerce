using Cortex.Mediator.Queries;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetShopProductsQuery : IQuery<(IList<ShopProductItemDto> Products, int Total, int TotalDisplay)>
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public string OrderBy { get; set; } = "Newest"; // PriceAsc, PriceDesc, Rating, Newest
        public string? SearchText { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public decimal? PriceFrom { get; set; }
        public decimal? PriceTo { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public string? TagName { get; set; }
    }
}

