using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Reviews;
using System.Collections.Generic;
using System.Linq;

namespace MaleFashion.Web.Models.Shop
{
    public class ShopDetailsModel
    {
        public Product Product { get; set; } = default!;
        public IList<Product> RelatedProducts { get; set; } = new List<Product>();
        public IList<ProductReview> Reviews { get; set; } = new List<ProductReview>();

        public decimal EffectivePrice => (Product.SalePrice ?? Product.BasePrice)?.Amount ?? 0m;
        public decimal BasePriceAmount => Product.BasePrice?.Amount ?? 0m;
        public bool IsOnSale => Product.SalePrice != null && Product.SalePrice.Amount < Product.BasePrice.Amount;

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

    public class ProductVariantColorItem
    {
        public string ColorHex { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
    }

    public class ProductVariantItemDto
    {
        public Guid Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal? Price { get; set; }
        public string Sku { get; set; } = string.Empty;
    }
}

