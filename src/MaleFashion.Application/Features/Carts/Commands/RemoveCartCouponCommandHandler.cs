using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class RemoveCartCouponCommandHandler : ICommandHandler<RemoveCartCouponCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public RemoveCartCouponCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveCartCouponCommand command, CancellationToken cancellationToken)
        {
            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                command.CustomerId, command.SessionId, cancellationToken);

            if (cart == null || string.IsNullOrWhiteSpace(cart.AppliedCouponCode))
                return false;

            cart.AppliedCouponCode = null;
            cart.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

