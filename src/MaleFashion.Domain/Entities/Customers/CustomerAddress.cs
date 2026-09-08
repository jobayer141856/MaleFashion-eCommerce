using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Customers
{
    public class CustomerAddress: IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public AddressType Type { get; set; }

        public Address Address { get; set; } = default!;
        public bool IsDefault { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
