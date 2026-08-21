using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Promotions
{
    public class DealCountdown : IAggregateRoot<Guid>
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string DealTitle { get; set; } = string.Empty;
        public Money SalePrice { get; set; } = default!;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
