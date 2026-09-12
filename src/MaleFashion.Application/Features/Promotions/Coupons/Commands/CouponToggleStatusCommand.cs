using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponToggleStatusCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

