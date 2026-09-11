using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Enums;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Customers.Commands
{
    public class CustomerAddressDto
    {
        public Guid? Id { get; set; }
        public AddressType Type { get; set; }
        public string StreetAddress { get; set; } = string.Empty;
        public string? ApartmentSuite { get; set; }
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = "Bangladesh";
        public bool IsDefault { get; set; }
    }

    public class CustomerUpdateCommand : ICommand<Customer>
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public List<CustomerAddressDto> Addresses { get; set; } = new();
    }
}