using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Wishlists.Commands
{
    public class RemoveWishlistItemCommandHandler : ICommandHandler<RemoveWishlistItemCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public RemoveWishlistItemCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(RemoveWishlistItemCommand command, CancellationToken cancellationToken)
        {
            if (command.ProductId == Guid.Empty)
                return false;

            if (command.CustomerId.HasValue && command.CustomerId.Value != Guid.Empty)
            {
                var customer = await _unitOfWork.CustomerRepository.GetCustomerWithWishlistByIdAsync(command.CustomerId.Value, cancellationToken);
                if (customer == null)
                    return false;

                var existingItem = customer.Wishlist.FirstOrDefault(w => w.ProductId == command.ProductId);
                if (existingItem != null)
                {
                    customer.Wishlist.Remove(existingItem);
                    await _unitOfWork.SaveAsync(cancellationToken);
                    return true;
                }
            }

            return true;
        }
    }
}
