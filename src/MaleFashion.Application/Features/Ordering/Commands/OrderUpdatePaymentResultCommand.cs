using Cortex.Mediator.Commands;
using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderUpdatePaymentResultCommand : ICommand<bool>
    {
        public string OrderNumber { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public string? TransactionId { get; set; }
        public string? GatewayNotes { get; set; }
    }
}

