using MaleFashion.Application.Features.Reviews.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Reviews;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Reviews
{
    public interface IProductReviewRepository : IRepository<ProductReview, Guid>
    {
        Task<(IList<ProductReview> Items, int Total, int TotalDisplay)> GetPagedReviews(GetAllProductReviewsByPagingQuery query, CancellationToken cancellationToken);
        Task<ProductReview?> GetReviewWithDetailsById(Guid id, CancellationToken cancellationToken);
        Task<(int Total, int Pending, int Approved, double AverageRating)> GetReviewStatsAsync(CancellationToken cancellationToken);
        Task RecalculateProductRatingAsync(Guid productId, CancellationToken cancellationToken);
    }
}

