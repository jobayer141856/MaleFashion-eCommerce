using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using System;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponUpdateCommand : ICommand<Coupon>
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinimumSpend { get; set; }
        public int? UsageLimit { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

