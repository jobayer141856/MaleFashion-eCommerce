using System;

namespace MaleFashion.Application.Features.Home.DTOs
{
    public class HomeHeroSlideDto
    {
        public Guid Id { get; set; }
        public string Subtitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ButtonText { get; set; } = "Shop now";
        public string TargetUrl { get; set; } = "/Shop";
    }
}

