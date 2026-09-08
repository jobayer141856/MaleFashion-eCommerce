using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Catalog
{
    public class ProductVariant : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public string Size { get; set; } = string.Empty; // XS, S, M, L, XL, XXL
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int StockQuantity { get; set; }

        // Optional price override for specific size/color
        public Money? VariantPrice { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
