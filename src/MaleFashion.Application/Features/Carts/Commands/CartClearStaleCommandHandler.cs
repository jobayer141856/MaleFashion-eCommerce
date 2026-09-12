using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Commands
{
    public class CartClearStaleCommandHandler : ICommandHandler<CartClearStaleCommand, int>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public CartClearStaleCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<int> Handle(CartClearStaleCommand command, CancellationToken cancellationToken)
        {
            var count = await _unitOfWork.CartRepository.ClearStaleCartsAsync(command.OlderThanDays, cancellationToken);
            return count;
        }
    }
}

