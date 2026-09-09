using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Customers
{
    public class WishlistItem : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
