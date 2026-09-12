using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponToggleStatusCommandHandler : ICommandHandler<CouponToggleStatusCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CouponToggleStatusCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CouponToggleStatusCommand command, CancellationToken cancellationToken)
        {
            var coupon = await _unitOfWork.CouponRepository.GetByIdAsync(command.Id, cancellationToken);
            if (coupon == null)
                throw new NotFoundException($"Coupon with ID '{command.Id}' not found.");

            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.CouponRepository.EditAsync(coupon, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return coupon.IsActive;
        }
    }
}

