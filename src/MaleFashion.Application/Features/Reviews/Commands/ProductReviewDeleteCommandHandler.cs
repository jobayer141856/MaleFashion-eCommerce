using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Reviews.Commands
{
    public class ProductReviewDeleteCommandHandler : ICommandHandler<ProductReviewDeleteCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ProductReviewDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ProductReviewDeleteCommand command, CancellationToken cancellationToken)
        {
            var review = await _unitOfWork.ProductReviewRepository.GetByIdAsync(command.Id, cancellationToken);
            if (review == null)
                throw new NotFoundException("Product review not found.");

            var productId = review.ProductId;
            _unitOfWork.ProductReviewRepository.Remove(review);
            await _unitOfWork.SaveAsync(cancellationToken);

            await _unitOfWork.ProductReviewRepository.RecalculateProductRatingAsync(productId, cancellationToken);

            return true;
        }
    }
}

