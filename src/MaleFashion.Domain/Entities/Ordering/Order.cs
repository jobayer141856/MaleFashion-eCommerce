using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MaleFashion.Domain.Entities.Ordering
{
    public class Order : IAggregateRoot<Guid>, IAuditableEntity
    {

        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;

        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public PaymentMethod PaymentMethod { get; set; }

        public Address BillingAddress { get; set; } = default!;
        public Address ShippingAddress { get; set; } = default!;

        public Money Subtotal { get; set; } = default!;
        public Money? ShippingCost { get; set; }
        public Money? DiscountAmount { get; set; }
        public Money TotalAmount { get; set; } = default!;

        public string? OrderNotes { get; set; }
        public string? TransactionId { get; set; } // Payment gateway reference
        
        public Guid? CouponId { get; set; }
        public Coupon? Coupon { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
