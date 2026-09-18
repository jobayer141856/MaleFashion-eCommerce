using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class RemoveCartCouponCommand : ICommand<bool>
    {
        public Guid? CustomerId { get; set; }
        public string? SessionId { get; set; }
    }
}

