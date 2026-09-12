using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Reviews;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Reviews.Queries
{
    public class GetProductReviewByIdQueryHandler : IQueryHandler<GetProductReviewByIdQuery, ProductReview?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetProductReviewByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductReview?> Handle(GetProductReviewByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductReviewRepository.GetReviewWithDetailsById(query.Id, cancellationToken);
        }
    }
}

