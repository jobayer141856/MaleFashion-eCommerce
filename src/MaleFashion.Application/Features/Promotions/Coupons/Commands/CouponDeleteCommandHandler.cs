using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Commands
{
    public class CouponDeleteCommandHandler : ICommandHandler<CouponDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CouponDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(CouponDeleteCommand command, CancellationToken cancellationToken)
        {
            var coupon = await _unitOfWork.CouponRepository.GetByIdAsync(command.Id, cancellationToken);
            if (coupon == null)
                throw new NotFoundException($"Coupon with ID '{command.Id}' not found.");

            if (coupon.UsedCount > 0)
                throw new InvalidOperationException($"Coupon '{coupon.Code}' has already been redeemed {coupon.UsedCount} time(s) in customer orders and cannot be deleted to preserve order history. Please deactivate it instead.");

            await _unitOfWork.CouponRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

