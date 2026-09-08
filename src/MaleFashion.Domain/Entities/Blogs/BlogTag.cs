using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Blogs
{
    public class BlogTag : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public BlogPost? BlogPost { get; set; }

        public string Name { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
