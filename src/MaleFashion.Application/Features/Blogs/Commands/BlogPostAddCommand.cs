using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Blogs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostAddCommand : ICommand<BlogPost>
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = "Admin";
        public string FeaturedImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public List<string> Tags { get; set; } = new();
    }
}