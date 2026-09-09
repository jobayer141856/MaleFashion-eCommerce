using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Carts;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Customers
{
    public class Customer : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Additional properties for the Customer entity
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<CustomerAddress> Addresses { get; set; } = new List<CustomerAddress>();
        public ICollection<WishlistItem> Wishlist { get; set; } = new List<WishlistItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();

        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();


    }
}
