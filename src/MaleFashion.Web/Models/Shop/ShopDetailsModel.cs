using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaleFashion.Web.Models.Shop
{
    public class ShopDetailsModel
    {
        public Product Product { get; set; } = default!;
        public IList<Product> RelatedProducts { get; set; } = new List<Product>();
        public IList<ProductReview> Reviews { get; set; } = new List<ProductReview>();
        public ProductActiveDealDto? ActiveDeal { get; set; }
        public Dictionary<Guid, ProductActiveDealDto> ActiveDeals { get; set; } = new();

        public decimal GetProductEffectivePrice(Product p)
        {
            decimal basePrice = p.BasePrice?.Amount ?? 0m;
            if (ActiveDeals != null && ActiveDeals.TryGetValue(p.Id, out var deal) && deal != null && deal.SalePrice > 0 && deal.SalePrice < basePrice)
            {
                return deal.SalePrice;
            }
            if (p.SalePrice != null && p.SalePrice.Amount < basePrice)
            {
                return p.SalePrice.Amount;
            }
            return basePrice;
        }

        public bool IsProductOnSale(Product p)
        {
            decimal basePrice = p.BasePrice?.Amount ?? 0m;
            if (basePrice <= 0) return false;

            return (ActiveDeals != null && ActiveDeals.TryGetValue(p.Id, out var deal) && deal != null && deal.SalePrice > 0 && deal.SalePrice < basePrice)
                || (p.SalePrice != null && p.SalePrice.Amount < basePrice);
        }

        public decimal BasePriceAmount => Product.BasePrice?.Amount ?? 0m;

        public decimal EffectivePrice
        {
            get
            {
                if (ActiveDeal != null && ActiveDeal.SalePrice > 0 && ActiveDeal.SalePrice < BasePriceAmount)
                {
                    return ActiveDeal.SalePrice;
                }

                if (Product.SalePrice != null && Product.SalePrice.Amount < BasePriceAmount)
                {
                    return Product.SalePrice.Amount;
                }

                return BasePriceAmount;
            }
        }

        public bool IsOnSale => (ActiveDeal != null && ActiveDeal.SalePrice > 0 && ActiveDeal.SalePrice < BasePriceAmount)
            || (Product.SalePrice != null && Product.SalePrice.Amount < BasePriceAmount);

        public decimal DiscountPercent
        {
            get
            {
                if (ActiveDeal != null && ActiveDeal.DiscountPercent > 0)
                {
                    return ActiveDeal.DiscountPercent;
                }

                if (IsOnSale && BasePriceAmount > 0)
                {
                    return Math.Round(((BasePriceAmount - EffectivePrice) / BasePriceAmount) * 100, 0);
                }

                return 0;
            }
        }

        public ProductImage? PrimaryImage => Product.Images?.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).FirstOrDefault();

        public List<ProductImage> GalleryImages => Product.Images != null && Product.Images.Any()
            ? Product.Images.OrderBy(i => i.DisplayOrder).ToList()
            : new List<ProductImage>();

        public List<string> AvailableSizes => Product.Variants != null
            ? Product.Variants.Where(v => v.StockQuantity > 0 && !string.IsNullOrWhiteSpace(v.Size))
                              .Select(v => v.Size)
                              .Distinct()
                              .ToList()
            : new List<string>();

        public List<ProductVariantColorItem> AvailableColors => Product.Variants != null
            ? Product.Variants.Where(v => v.StockQuantity > 0 && !string.IsNullOrWhiteSpace(v.ColorHex))
                              .GroupBy(v => v.ColorHex)
                              .Select(g => new ProductVariantColorItem
                              {
                                  ColorHex = g.Key,
                                  ColorName = g.First().ColorName
                              })
                              .ToList()
            : new List<ProductVariantColorItem>();

        public string Currency => "BDT";

        public ShopReviewSubmitModel ReviewInput { get; set; } = new();

        public List<ProductVariantItemDto> VariantItems => Product.Variants != null
            ? Product.Variants.Select(v => new ProductVariantItemDto
            {
                Id = v.Id,
                Size = v.Size ?? string.Empty,
                ColorName = v.ColorName ?? string.Empty,
                ColorHex = v.ColorHex ?? string.Empty,
                StockQuantity = v.StockQuantity,
                Price = v.VariantPrice?.Amount,
                Sku = v.Sku ?? string.Empty
            }).ToList()
            : new List<ProductVariantItemDto>();
    }
    
}

