using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class ApplyCartCouponCommand : ICommand<(bool Success, string Message)>
    {
        public Guid? CustomerId { get; set; }
        public string? SessionId { get; set; }
        public string CouponCode { get; set; } = string.Empty;
    }
}

