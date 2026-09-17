using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class UpdateCartItemQuantityCommandHandler : ICommandHandler<UpdateCartItemQuantityCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public UpdateCartItemQuantityCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
        {
            if (command.ItemQuantities == null || !command.ItemQuantities.Any())
                return true;

            var cart = await _unitOfWork.CartRepository.GetActiveCartByCustomerOrSessionAsync(
                command.CustomerId, command.SessionId, cancellationToken);

            if (cart == null)
                return false;

            var itemsToRemove = cart.Items.Where(i => command.ItemQuantities.TryGetValue(i.Id, out var qty) && qty <= 0).ToList();
            foreach (var item in itemsToRemove)
            {
                cart.Items.Remove(item);
            }

            foreach (var item in cart.Items)
            {
                if (command.ItemQuantities.TryGetValue(item.Id, out var qty) && qty > 0)
                {
                    item.Quantity = qty;
                    item.UpdatedAt = DateTime.UtcNow;
                }
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

