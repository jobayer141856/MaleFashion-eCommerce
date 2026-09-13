using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderUpdateStatusCommandHandler : ICommandHandler<OrderUpdateStatusCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public OrderUpdateStatusCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(OrderUpdateStatusCommand command, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(command.Id, cancellationToken);
            if (order == null)
                throw new NotFoundException("Order not found.");

            order.Status = command.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

