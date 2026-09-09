using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;


namespace MaleFashion.Application.Features.Blogs.Commands
{
    public class BlogPostAddCommand : ICommand<BlogPost>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        public string FeaturedImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public List<string> Tags { get; set; } = new List<string>();
        public List<string> Comments { get; set; } = new List<string>();

    }
}
