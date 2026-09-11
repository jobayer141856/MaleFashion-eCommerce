using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Customers;
using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Application.Features.Customers.Commands
{
    public class CustomerAddCommand : ICommand<Customer>
    {
        public Guid? UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        public CustomerOrigin Origin { get; set; } = CustomerOrigin.Admin;

        // Optional address
        public string? StreetAddress { get; set; }
        public string? ApartmentSuite { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}