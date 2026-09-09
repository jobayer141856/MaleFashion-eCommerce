using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Promotions
{
    public class DealCountdown : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        public string DealTitle { get; set; } = string.Empty;
        public Money SalePrice { get; set; } = default!;
        public DateTime? StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
