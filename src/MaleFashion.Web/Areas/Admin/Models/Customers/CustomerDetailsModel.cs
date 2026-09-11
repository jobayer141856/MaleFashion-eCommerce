using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Areas.Admin.Models.Customers
{
    public class CustomerAddressItemModel
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string FullAddress { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }

    public class CustomerOrderItemModel
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class CustomerDetailsModel
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Gender { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; }

        public CustomerOrigin Origin { get; set; }

        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }

        public List<CustomerAddressItemModel> Addresses { get; set; } = new();
        public List<CustomerOrderItemModel> RecentOrders { get; set; } = new();
    }
}