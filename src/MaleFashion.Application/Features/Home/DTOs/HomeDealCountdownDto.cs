using System;

namespace MaleFashion.Application.Features.Home.DTOs
{
    public class HomeDealCountdownDto
    {
        public Guid? DealId { get; set; }
        public Guid? ProductId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string SectionHeading { get; set; } = "Deal Of The Week";
        public decimal SalePrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string Currency { get; set; } = "BDT";
        public DateTime EndDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = "/Shop";
    }
}

