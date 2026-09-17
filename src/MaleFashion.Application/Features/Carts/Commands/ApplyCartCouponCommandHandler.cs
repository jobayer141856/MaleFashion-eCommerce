using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class ApplyCartCouponCommandHandler : ICommandHandler<ApplyCartCouponCommand, (bool Success, string Message)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ApplyCartCouponCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> Handle(ApplyCartCouponCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.CouponCode))
                return (false, "Please provide a valid coupon code.");

            var code = command.CouponCode.Trim();
            var now = DateTime.UtcNow;

            var coupons = await _unitOfWork.CouponRepository.GetAllAsync(cancellationToken);
            var coupon = coupons.FirstOrDefault(c =>
                string.Equals(c.Code, code, StringComparison.OrdinalIgnoreCase) &&
                c.IsActive &&
                (!c.StartDate.HasValue || c.StartDate <= now) &&
                c.ExpirationDate >= now);

            if (coupon == null)
                return (false, "Invalid or expired coupon code.");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
                return (false, "Coupon usage limit has been reached.");

            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                command.CustomerId, command.SessionId, cancellationToken);

            if (cart == null || !cart.Items.Any())
                return (false, "Your shopping cart is currently empty.");

            decimal subtotal = cart.Items.Sum(i => i.Quantity * i.UnitPrice.Amount);
            if (coupon.MinimumSpend.HasValue && subtotal < coupon.MinimumSpend.Value)
                return (false, $"Minimum spend of BDT {coupon.MinimumSpend.Value:N2} is required to apply this coupon.");

            cart.AppliedCouponCode = coupon.Code;
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);

            return (true, "Coupon applied successfully!");
        }
    }
}

