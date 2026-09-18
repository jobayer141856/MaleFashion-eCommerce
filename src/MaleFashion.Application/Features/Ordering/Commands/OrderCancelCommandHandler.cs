using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Enums;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Commands
{
    public class OrderCancelCommandHandler : ICommandHandler<OrderCancelCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public OrderCancelCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(OrderCancelCommand command, CancellationToken cancellationToken)
        {
            var order = await _unitOfWork.OrderRepository.GetOrderWithDetailsById(command.Id, cancellationToken);
            if (order == null)
                throw new NotFoundException("Order not found.");

            if (order.Status == OrderStatus.Cancelled)
                return true;

            order.Status = OrderStatus.Cancelled;
            if (!string.IsNullOrWhiteSpace(command.Reason))
            {
                order.OrderNotes = string.IsNullOrWhiteSpace(order.OrderNotes)
                    ? $"Cancellation Reason: {command.Reason}"
                    : $"{order.OrderNotes}\nCancellation Reason: {command.Reason}";
            }
            order.UpdatedAt = DateTime.UtcNow;

            // Restore product variant stock quantity
            foreach (var item in order.OrderItems)
            {
                if (item.ProductVariant != null)
                {
                    item.ProductVariant.StockQuantity += item.Quantity;
                }
            }

            // Restore coupon redemption count if applicable
            if (order.CouponId.HasValue)
            {
                var coupon = await _unitOfWork.CouponRepository.GetByIdAsync(order.CouponId.Value, cancellationToken);
                if (coupon != null && coupon.UsedCount > 0)
                {
                    coupon.UsedCount--;
                }
            }

            await _unitOfWork.SaveAsync(cancellationToken);
            return true;
        }
    }
}

