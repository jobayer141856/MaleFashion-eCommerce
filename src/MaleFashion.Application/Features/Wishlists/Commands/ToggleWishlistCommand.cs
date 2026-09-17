using Cortex.Mediator.Commands;
using MaleFashion.Application.Features.Wishlists.DTOs;
using System;

namespace MaleFashion.Application.Features.Wishlists.Commands
{
    public class ToggleWishlistCommand : ICommand<ToggleWishlistResultDto>
    {
        public Guid? CustomerId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? VariantId { get; set; }
    }
}

