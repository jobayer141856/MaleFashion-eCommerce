using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderUpdatePaymentResultCommandHandler : ICommandHandler<OrderUpdatePaymentResultCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public OrderUpdatePaymentResultCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(OrderUpdatePaymentResultCommand command, CancellationToken cancellationToken)
        {
            var orders = await _unitOfWork.OrderRepository.GetAllAsync(cancellationToken);
            var order = orders.FirstOrDefault(o => o.OrderNumber == command.OrderNumber);
            if (order == null)
            {
                return false;
            }

            order.PaymentStatus = command.PaymentStatus;
            if (!string.IsNullOrWhiteSpace(command.TransactionId))
            {
                order.TransactionId = command.TransactionId;
            }

            if (!string.IsNullOrWhiteSpace(command.GatewayNotes))
            {
                order.OrderNotes = string.IsNullOrWhiteSpace(order.OrderNotes)
                    ? command.GatewayNotes
                    : $"{order.OrderNotes}\n{command.GatewayNotes}";
            }

            order.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

