using System;

namespace MaleFashion.Application.Features.Home.DTOs
{
    public class HomeCategoryBannerDto
    {
        public Guid? CategoryId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = "/Shop";
        public string ColumnClass { get; set; } = "col-lg-7";
        public string CssModifier { get; set; } = string.Empty;
    }
}

