using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Home
{
    public class IndexModel
    {
        public List<HomeHeroSlideModel> HeroSlides { get; set; } = new();
        public List<HomeBannerModel> Banners { get; set; } = new();
        public List<HomeProductCardModel> FeaturedProducts { get; set; } = new();
        public HomeDealCountdownModel DealOfTheWeek { get; set; } = new();
        public List<HomeBlogCardModel> LatestBlogs { get; set; } = new();
        public List<HomeInstagramModel> InstagramPosts { get; set; } = new();
    }
}

