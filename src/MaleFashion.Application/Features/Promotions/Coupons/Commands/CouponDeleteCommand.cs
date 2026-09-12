using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

