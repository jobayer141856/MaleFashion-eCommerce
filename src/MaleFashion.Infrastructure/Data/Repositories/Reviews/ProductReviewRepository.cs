using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Features.Reviews.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Reviews;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Reviews
{
    public class ProductReviewRepository : Repository<ProductReview, Guid>, IProductReviewRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<ProductReview> Items, int Total, int TotalDisplay)> GetPagedReviews(GetAllProductReviewsByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "CreatedAt desc" : query.SortText;

            return await GetDynamicAsync(
                x => (string.IsNullOrWhiteSpace(query.SearchText) ||
                      x.ReviewerName.Contains(query.SearchText) ||
                      x.ReviewerEmail.Contains(query.SearchText) ||
                      x.Title.Contains(query.SearchText) ||
                      x.Comment.Contains(query.SearchText) ||
                      (x.Product != null && x.Product.Name.Contains(query.SearchText))) &&
                     (!query.StatusFilter.HasValue || (query.StatusFilter.Value ? x.IsApproved : !x.IsApproved)) &&
                     (!query.RatingFilter.HasValue || x.Rating == query.RatingFilter.Value) &&
                     (!query.VerifiedOnly.HasValue || !query.VerifiedOnly.Value || x.IsVerifiedPurchase),
                sortText,
                q => q.Include(r => r.Product).ThenInclude(p => p.Images).Include(r => r.Customer),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<ProductReview?> GetReviewWithDetailsById(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<ProductReview>()
                .Include(r => r.Product)
                    .ThenInclude(p => p.Images)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
        }

        public async Task<(int Total, int Pending, int Approved, double AverageRating)> GetReviewStatsAsync(CancellationToken cancellationToken)
        {
            var total = await GetCountAsync(null, cancellationToken);
            var pending = await GetCountAsync(x => !x.IsApproved, cancellationToken);
            var approved = await GetCountAsync(x => x.IsApproved, cancellationToken);

            double avgRating = 0;
            if (approved > 0)
            {
                avgRating = await _context.Set<ProductReview>()
                    .Where(r => r.IsApproved)
                    .AverageAsync(r => (double)r.Rating, cancellationToken);
            }

            return (total, pending, approved, Math.Round(avgRating, 1));
        }

        public async Task RecalculateProductRatingAsync(Guid productId, CancellationToken cancellationToken)
        {
            var approvedQuery = _context.Set<ProductReview>().Where(r => r.ProductId == productId && r.IsApproved);
            var hasApproved = await approvedQuery.AnyAsync(cancellationToken);

            double newRating = 0;
            if (hasApproved)
            {
                newRating = Math.Round(await approvedQuery.AverageAsync(r => (double)r.Rating, cancellationToken), 1);
            }

            var product = await _context.Set<Product>().FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
            if (product != null)
            {
                product.Rating = newRating;
                await _context.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

