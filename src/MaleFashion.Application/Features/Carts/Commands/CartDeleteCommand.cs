using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class CartDeleteCommand : ICommand<bool>
    {
        public Guid Id { get; set; }

        public CartDeleteCommand(Guid id)
        {
            Id = id;
        }

        public CartDeleteCommand() { }
    }
}

