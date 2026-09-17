using Cortex.Mediator.Commands;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class UpdateCartItemQuantityCommand : ICommand<bool>
    {
        public Guid? CustomerId { get; set; }
        public string? SessionId { get; set; }
        public Dictionary<Guid, int> ItemQuantities { get; set; } = new();
    }
}

