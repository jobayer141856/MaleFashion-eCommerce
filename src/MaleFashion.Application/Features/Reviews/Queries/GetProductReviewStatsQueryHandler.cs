using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Reviews.Queries
{
    public class GetProductReviewStatsQueryHandler : IQueryHandler<GetProductReviewStatsQuery, (int Total, int Pending, int Approved, double AverageRating)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetProductReviewStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(int Total, int Pending, int Approved, double AverageRating)> Handle(GetProductReviewStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductReviewRepository.GetReviewStatsAsync(cancellationToken);
        }
    }
}

