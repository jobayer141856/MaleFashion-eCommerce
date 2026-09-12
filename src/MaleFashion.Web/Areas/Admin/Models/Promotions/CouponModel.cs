using MaleFashion.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Promotions
{
    public class CouponModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Coupon code is required.")]
        [StringLength(50, ErrorMessage = "Coupon code cannot exceed 50 characters.")]
        [Display(Name = "Coupon Code")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Discount type is required.")]
        [Display(Name = "Discount Type")]
        public DiscountType DiscountType { get; set; } = DiscountType.Percentage;

        [Required(ErrorMessage = "Discount value is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Discount value must be greater than zero.")]
        [Display(Name = "Discount Value")]
        public decimal DiscountValue { get; set; }

        [Range(0, 1000000, ErrorMessage = "Minimum spend must be positive.")]
        [Display(Name = "Minimum Spend (Optional)")]
        public decimal? MinimumSpend { get; set; }

        [Range(1, 1000000, ErrorMessage = "Usage limit must be at least 1.")]
        [Display(Name = "Total Usage Limit (Optional)")]
        public int? UsageLimit { get; set; }

        [Display(Name = "Used Count")]
        public int UsedCount { get; set; }

        [Display(Name = "Start Date (Optional)")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Expiration date is required.")]
        [Display(Name = "Expiration Date")]
        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}

