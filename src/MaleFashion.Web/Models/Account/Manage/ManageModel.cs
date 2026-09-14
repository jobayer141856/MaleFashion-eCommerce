using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Account.Manage
{
    public class ManageModel
    {
        public Guid CustomerId { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public bool IsEmailConfirmed { get; set; }

        public CustomerAddressViewModel? DefaultShippingAddress { get; set; }
        public CustomerAddressViewModel? DefaultBillingAddress { get; set; }
        public List<CustomerAddressViewModel> Addresses { get; set; } = new();
        public List<CustomerOrderSummaryViewModel> RecentOrders { get; set; } = new();

        public int TotalOrders { get; set; }
        public int TotalWishlistItems { get; set; }
        public int TotalReviews { get; set; }
    }
}

