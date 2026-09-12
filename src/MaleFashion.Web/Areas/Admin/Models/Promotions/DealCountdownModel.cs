using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Promotions
{
    public class DealCountdownModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Please select a product for the flash deal.")]
        [Display(Name = "Featured Product")]
        public Guid ProductId { get; set; }

        public string? ProductName { get; set; }
        public string? ProductSku { get; set; }
        public decimal? ProductBasePrice { get; set; }

        [Required(ErrorMessage = "Deal campaign title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        [Display(Name = "Deal Campaign Title")]
        public string DealTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Flash deal sale price is required.")]
        [Range(0.01, 1000000, ErrorMessage = "Sale price must be greater than zero.")]
        [Display(Name = "Flash Deal Sale Price")]
        public decimal SalePrice { get; set; }

        public string Currency { get; set; } = "BDT";

        [Display(Name = "Start Date (Optional)")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "Deal countdown end date is required.")]
        [Display(Name = "Deal Countdown End Date")]
        public DateTime EndDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Display(Name = "Active Status")]
        public bool IsActive { get; set; } = true;
    }
}

