using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Promotions;
using System;

namespace MaleFashion.Application.Features.Promotions.Coupons.Queries
{
    public class GetCouponByIdQuery : IQuery<Coupon?>
    {
        public Guid Id { get; set; }
    }
}

