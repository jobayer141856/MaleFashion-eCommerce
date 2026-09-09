using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Carts
{
    public class CartItem : IAuditableEntity
    {
        public Guid Id { get; set; }

        public Guid CartId { get; set; }
        public Cart? Cart { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        // Nullable so products without variants can be added
        public Guid? ProductVariantId { get; set; }
        public ProductVariant? ProductVariant { get; set; }

        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public Money UnitPrice { get; set; } = default!;
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

    }
}
