using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Wishlists.Commands
{
    public class RemoveWishlistItemCommand : ICommand<bool>
    {
        public Guid? CustomerId { get; set; }
        public Guid ProductId { get; set; }
    }
}

