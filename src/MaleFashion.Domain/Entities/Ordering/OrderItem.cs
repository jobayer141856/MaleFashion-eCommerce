using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Ordering
{
    public class OrderItem : IAuditableEntity
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }
        public Order? Order { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        // Nullable so products without variants can be ordered
        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public Money UnitPrice { get; set; } = default!;
        public int Quantity { get; set; }
        public Money TotalPrice { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
