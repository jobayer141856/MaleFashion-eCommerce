using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Customers
{
    public class WishlistItem 
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public DateTime AddedDate { get; set; } = DateTime.UtcNow;
    }
}
