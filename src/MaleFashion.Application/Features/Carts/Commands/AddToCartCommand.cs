using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class AddToCartCommand : ICommand<bool>
    {
        public Guid? CustomerId { get; set; }
        public string? SessionId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? ProductVariantId { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int Quantity { get; set; } = 1;
    }
}

