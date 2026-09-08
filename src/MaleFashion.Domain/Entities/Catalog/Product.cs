using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Reviews;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Catalog
{
    public class Product : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AdditionalInformation { get; set; }
        public string Sku { get; set; } = string.Empty;

        public Money BasePrice { get; set; } = default!;
        public Money? SalePrice { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public Guid BrandId { get; set; }
        public Brand? Brand { get; set; }

        public bool IsNewArrival { get; set; }
        public bool IsHotSale { get; set; }
        public bool IsActive { get; set; } = true;
        public double Rating { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductTag> Tags { get; set; } = new List<ProductTag>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}
