using Cortex.Mediator.Commands;
using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderUpdatePaymentStatusCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string? TransactionId { get; set; }

        public OrderUpdatePaymentStatusCommand(Guid id, PaymentStatus paymentStatus, string? transactionId = null)
        {
            Id = id;
            PaymentStatus = paymentStatus;
            TransactionId = transactionId;
        }

        public OrderUpdatePaymentStatusCommand() { }
    }
}

