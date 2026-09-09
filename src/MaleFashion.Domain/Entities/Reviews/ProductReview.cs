using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Reviews
{
    public class ProductReview : IAggregateRoot<Guid>, IAuditableEntity
    {
        public Guid Id { get; set; }
     
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }
       
        public Guid? CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        
        public int Rating { get; set; }
        
        public string Title { get; set; } = string.Empty;   // e.g. "Great quality and fit!"
        public string Comment { get; set; } = string.Empty; // Detailed feedback
       
        public bool IsVerifiedPurchase { get; set; } // True if customer actually ordered this item
        public bool IsApproved { get; set; } = false; // Moderation: Admin approves before it goes public
        public int HelpfulCount { get; set; } = 0;   // "Was this review helpful? (12 people found this helpful)"
   
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
