using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Catalog
{
    public class Category : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public string? ImageName { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Category> SubCategories { get; set; } = new List<Category>();
       
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}
