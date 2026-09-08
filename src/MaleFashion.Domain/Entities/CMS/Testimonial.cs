using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.CMS
{
    public class Testimonial : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorProfession { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
        public string Quote { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
