using MaleFashion.Domain.Contracts;
using System;

namespace MaleFashion.Domain.Entities.Blogs
{
    public class BlogComment : IAggregateRoot<Guid>,IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid BlogPostId { get; set; }
        public BlogPost? BlogPost { get; set; }

        public Guid? UserId { get; set; } 

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false; 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}