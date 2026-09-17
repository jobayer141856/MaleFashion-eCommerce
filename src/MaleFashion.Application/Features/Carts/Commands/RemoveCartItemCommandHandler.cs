using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class RemoveCartItemCommandHandler : ICommandHandler<RemoveCartItemCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public RemoveCartItemCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveCartItemCommand command, CancellationToken cancellationToken)
        {
            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                command.CustomerId, command.SessionId, cancellationToken);

            if (cart == null)
                return false;

            var item = cart.Items.FirstOrDefault(i => i.Id == command.CartItemId);
            if (item == null)
                return false;

            cart.Items.Remove(item);
            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

