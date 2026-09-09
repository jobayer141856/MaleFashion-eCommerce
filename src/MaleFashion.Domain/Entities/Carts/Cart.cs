using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Carts
{
    public class Cart : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public string SessionId { get; set; } = string.Empty;
        public string? AppliedCouponCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
