using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Reviews;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Reviews.Queries
{
    public class GetAllProductReviewsByPagingQueryHandler : IQueryHandler<GetAllProductReviewsByPagingQuery, (IList<ProductReview>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllProductReviewsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<ProductReview>, int, int)> Handle(GetAllProductReviewsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductReviewRepository.GetPagedReviews(query, cancellationToken);
        }
    }
}

