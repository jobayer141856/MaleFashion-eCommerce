using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderDeleteCommandHandler : ICommandHandler<OrderDeleteCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public OrderDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(OrderDeleteCommand command, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetByIdAsync(command.Id, cancellationToken);
            if (order == null)
                throw new NotFoundException("Order not found.");

            // Safeguard: Cannot delete fulfilled or active orders
            if (order.Status == OrderStatus.Delivered || order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Processing)
            {
                throw new InvalidOperationException($"Orders in '{order.Status}' status cannot be deleted. Please cancel the order first.");
            }

            _unitOfWork.OrderRepository.Remove(order);
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

