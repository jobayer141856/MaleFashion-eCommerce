using MaleFashion.Domain.Contracts;
using System;

namespace MaleFashion.Domain.Entities.CMS
{
    public class HeroSlide : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Subtitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ButtonText { get; set; } = "Shop now";
        public string TargetUrl { get; set; } = "/Shop";
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

