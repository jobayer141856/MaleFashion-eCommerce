using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Reviews;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Reviews.Commands
{
    public class ProductReviewAddCustomerCommandHandler : ICommandHandler<ProductReviewAddCustomerCommand, (bool Success, string Message)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ProductReviewAddCustomerCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message)> Handle(ProductReviewAddCustomerCommand command, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product == null)
                return (false, "Product not found.");

            var review = new ProductReview
            {
                Id = Guid.NewGuid(),
                ProductId = command.ProductId,
                CustomerId = command.CustomerId,
                ReviewerName = command.ReviewerName.Trim(),
                ReviewerEmail = command.ReviewerEmail.Trim(),
                Rating = Math.Clamp(command.Rating, 1, 5),
                Title = command.Title.Trim(),
                Comment = command.Comment.Trim(),
                IsApproved = false,
                IsVerifiedPurchase = false,
                HelpfulCount = 0,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProductReviewRepository.AddAsync(review, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return (true, "Thank you! Your review has been submitted and will appear once approved.");
        }
    }
}

