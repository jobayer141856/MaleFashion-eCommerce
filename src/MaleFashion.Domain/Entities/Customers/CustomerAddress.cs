using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Domain.Entities.Customers
{
    public class CustomerAddress 
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string AddressType { get; set; } = "Shipping"; // Billing or Shipping
        public string StreetAddress { get; set; } = string.Empty;
        public string? ApartmentSuite { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
    }
}
