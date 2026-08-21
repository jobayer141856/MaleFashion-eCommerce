using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Blogs
{
    public class BlogPost : IAggregateRoot<Guid>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
        public string FeaturedImageUrl { get; set; } = string.Empty;

        public ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
        public ICollection<BlogTag> Tags { get; set; } = new List<BlogTag>();
    }
}
