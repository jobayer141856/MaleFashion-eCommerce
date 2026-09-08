using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Blogs
{
    public class BlogPost : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        public string FeaturedImageUrl { get; set; } = string.Empty;
        public bool IsPublished { get; set; } = true;

        public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
        public ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
