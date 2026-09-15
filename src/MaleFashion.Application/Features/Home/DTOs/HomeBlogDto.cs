using System;

namespace MaleFashion.Application.Features.Home.DTOs
{
    public class HomeBlogDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = "/BlogPosts";
    }
}

