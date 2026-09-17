using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class RemoveCartItemCommand : ICommand<bool>
    {
        public Guid? CustomerId { get; set; }
        public string? SessionId { get; set; }
        public Guid CartItemId { get; set; }
    }
}

