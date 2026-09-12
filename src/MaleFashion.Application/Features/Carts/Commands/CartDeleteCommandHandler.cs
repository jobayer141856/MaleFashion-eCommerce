using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class CartDeleteCommandHandler : ICommandHandler<CartDeleteCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CartDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(CartDeleteCommand command, CancellationToken cancellationToken)
        {
            var cart = await _unitOfWork.CartRepository.GetByIdAsync(command.Id, cancellationToken);
            if (cart == null)
                throw new NotFoundException("Cart not found.");

            _unitOfWork.CartRepository.Remove(cart);
            await _unitOfWork.SaveAsync(cancellationToken);

            return true;
        }
    }
}

