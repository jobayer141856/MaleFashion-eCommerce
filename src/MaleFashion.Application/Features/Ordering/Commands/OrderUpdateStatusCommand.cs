using Cortex.Mediator.Commands;
using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderUpdateStatusCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
        public OrderStatus Status { get; set; }

        public OrderUpdateStatusCommand(Guid id, OrderStatus status)
        {
            Id = id;
            Status = status;
        }

        public OrderUpdateStatusCommand() { }
    }
}

