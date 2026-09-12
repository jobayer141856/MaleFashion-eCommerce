using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponUpdateCommandHandler : ICommandHandler<CouponUpdateCommand, Coupon>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CouponUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Coupon> Handle(CouponUpdateCommand command, CancellationToken cancellationToken)
        {
            var coupon = await _unitOfWork.CouponRepository.GetByIdAsync(command.Id, cancellationToken);
            if (coupon == null)
                throw new NotFoundException($"Coupon with ID '{command.Id}' not found.");

            var normalizedCode = command.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedCode))
                throw new InvalidOperationException("Coupon code cannot be empty.");

            var isDuplicate = await _unitOfWork.CouponRepository.IsDuplicateCode(normalizedCode, command.Id, cancellationToken);
            if (isDuplicate)
                throw new DuplicateDataException($"Coupon code '{normalizedCode}' is already in use by another coupon.");

            if (command.DiscountType == DiscountType.Percentage && (command.DiscountValue <= 0 || command.DiscountValue > 100))
                throw new InvalidOperationException("Percentage discount must be between 1% and 100%.");

            if (command.DiscountType == DiscountType.FixedAmount && command.DiscountValue <= 0)
                throw new InvalidOperationException("Fixed amount discount must be greater than zero.");

            if (command.StartDate.HasValue && command.ExpirationDate <= command.StartDate.Value)
                throw new InvalidOperationException("Expiration date must be later than start date.");

            coupon.Code = normalizedCode;
            coupon.DiscountType = command.DiscountType;
            coupon.DiscountValue = command.DiscountValue;
            coupon.MinimumSpend = command.MinimumSpend;
            coupon.UsageLimit = command.UsageLimit;
            coupon.StartDate = command.StartDate;
            coupon.ExpirationDate = command.ExpirationDate;
            coupon.IsActive = command.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CouponRepository.EditAsync(coupon, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return coupon;
        }
    }
}

