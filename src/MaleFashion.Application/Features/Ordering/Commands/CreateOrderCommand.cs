using Cortex.Mediator.Commands;
using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.ValueOfObjects;
using System;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class CreateOrderCommand : ICommand<CreateOrderResultDto>
    {
        public Guid? CustomerId { get; set; }
        public string SessionId { get; set; } = string.Empty;

        // Customer Details
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Addresses
        public Address ShippingAddress { get; set; } = default!;
        public Address BillingAddress { get; set; } = default!;

        // Order Details
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.SSLCommerz;
        public string? OrderNotes { get; set; }
    }
}

