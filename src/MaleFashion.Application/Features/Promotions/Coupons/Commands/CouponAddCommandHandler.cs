using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponAddCommandHandler : ICommandHandler<CouponAddCommand, Coupon>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CouponAddCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Coupon> Handle(CouponAddCommand command, CancellationToken cancellationToken)
        {
            var normalizedCode = command.Code?.Trim().ToUpperInvariant() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedCode))
                throw new InvalidOperationException("Coupon code cannot be empty.");

            var isDuplicate = await _unitOfWork.CouponRepository.IsDuplicateCode(normalizedCode, null, cancellationToken);
            if (isDuplicate)
                throw new DuplicateDataException($"Coupon code '{normalizedCode}' already exists.");

            if (command.DiscountType == DiscountType.Percentage && (command.DiscountValue <= 0 || command.DiscountValue > 100))
                throw new InvalidOperationException("Percentage discount must be between 1% and 100%.");

            if (command.DiscountType == DiscountType.FixedAmount && command.DiscountValue <= 0)
                throw new InvalidOperationException("Fixed amount discount must be greater than zero.");

            if (command.StartDate.HasValue && command.ExpirationDate <= command.StartDate.Value)
                throw new InvalidOperationException("Expiration date must be later than start date.");

            var coupon = new Coupon
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                Code = normalizedCode,
                DiscountType = command.DiscountType,
                DiscountValue = command.DiscountValue,
                MinimumSpend = command.MinimumSpend,
                UsageLimit = command.UsageLimit,
                UsedCount = 0,
                StartDate = command.StartDate,
                ExpirationDate = command.ExpirationDate,
                IsActive = command.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.CouponRepository.AddAsync(coupon, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return coupon;
        }
    }
}

