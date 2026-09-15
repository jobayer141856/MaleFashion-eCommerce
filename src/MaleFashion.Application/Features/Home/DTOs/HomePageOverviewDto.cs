using System.Collections.Generic;

namespace MaleFashion.Application.Features.Home.DTOs
{
    public class HomePageOverviewDto
    {
        public List<HomeHeroSlideDto> HeroSlides { get; set; } = new();
        public List<HomeCategoryBannerDto> Banners { get; set; } = new();
        public List<HomeProductDto> FeaturedProducts { get; set; } = new();
        public HomeDealCountdownDto DealOfTheWeek { get; set; } = new();
        public List<HomeBlogDto> LatestBlogs { get; set; } = new();
        public List<HomeInstagramDto> InstagramPosts { get; set; } = new();
    }
}

