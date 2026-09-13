using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderCancelCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
        public string? Reason { get; set; }

        public OrderCancelCommand(Guid id, string? reason = null)
        {
            Id = id;
            Reason = reason;
        }

        public OrderCancelCommand() { }
    }
}

