using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Blogs
{
    public class BlogTag 
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
