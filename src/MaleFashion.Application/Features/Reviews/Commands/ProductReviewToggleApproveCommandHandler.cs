using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Reviews.Commands
{
    public class ProductReviewToggleApproveCommandHandler : ICommandHandler<ProductReviewToggleApproveCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ProductReviewToggleApproveCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ProductReviewToggleApproveCommand command, CancellationToken cancellationToken)
        {
            var review = await _unitOfWork.ProductReviewRepository.GetByIdAsync(command.Id, cancellationToken);
            if (review == null)
                throw new NotFoundException("Product review not found.");

            review.IsApproved = !review.IsApproved;
            review.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync(cancellationToken);

            await _unitOfWork.ProductReviewRepository.RecalculateProductRatingAsync(review.ProductId, cancellationToken);

            return review.IsApproved;
        }
    }
}

