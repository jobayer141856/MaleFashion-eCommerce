using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Catalogs
{
    public class ProductVariantItemModel
    {
        public Guid? Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#000000";
        public string? Sku { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockQuantity { get; set; }
        public decimal? Price { get; set; }
    }

    public class ProductImageItemModel
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
    }

    public class ProductModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(250, ErrorMessage = "Product name cannot exceed 250 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "SKU is required.")]
        [StringLength(100, ErrorMessage = "SKU cannot exceed 100 characters.")]
        public string Sku { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Additional Information")]
        public string? AdditionalInformation { get; set; }

        [Required(ErrorMessage = "Base price is required.")]
        [Range(0.01, 10000000, ErrorMessage = "Base price must be greater than 0.")]
        [Display(Name = "Base Price (BDT)")]
        public decimal BasePrice { get; set; }

        [Range(0.01, 10000000, ErrorMessage = "Sale price must be greater than 0.")]
        [Display(Name = "Sale Price (BDT)")]
        public decimal? SalePrice { get; set; }

        [Required(ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public Guid CategoryId { get; set; }
        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Please select a brand.")]
        [Display(Name = "Brand")]
        public Guid BrandId { get; set; }
        public string? BrandName { get; set; }

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;

        [Display(Name = "New Arrival")]
        public bool IsNewArrival { get; set; }

        [Display(Name = "Hot Sale")]
        public bool IsHotSale { get; set; }

        [Display(Name = "Tags (comma-separated)")]
        public string? TagNamesCsv { get; set; }

        // Primary Image
        [Display(Name = "Primary Image")]
        public IFormFile? PrimaryImage { get; set; }
        public string? PrimaryImageUrl { get; set; }

        // Gallery Images
        [Display(Name = "Gallery Images")]
        public List<IFormFile>? GalleryImages { get; set; }
        public List<ProductImageItemModel> ExistingGalleryImages { get; set; } = new();

        // Variants
        public List<ProductVariantItemModel> Variants { get; set; } = new();
    }
}

