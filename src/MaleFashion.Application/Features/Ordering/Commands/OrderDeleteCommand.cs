using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderDeleteCommand : ICommand<bool>
    {
        public Guid Id { get; set; }

        public OrderDeleteCommand(Guid id)
        {
            Id = id;
        }

        public OrderDeleteCommand() { }
    }
}

