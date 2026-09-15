using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.DealCountdowns.Queries;
using MaleFashion.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Promotions
{
    public class DealCountdownRepository : Repository<DealCountdown, Guid>, IDealCountdownRepository
    {
        private readonly ApplicationDbContext _context;

        public DealCountdownRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<DealCountdown>, int, int)> GetPagedDealCountdowns(GetAllDealCountdownsByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "EndDate desc" : query.SortText;
            if (sortText.Contains("SalePrice") && !sortText.Contains("SalePrice.Amount"))
            {
                sortText = sortText.Replace("SalePrice", "SalePrice.Amount");
            }

            return await GetDynamicAsync(
                x => query.SearchText == null || x.DealTitle.Contains(query.SearchText) || (x.Product != null && x.Product.Name.Contains(query.SearchText)),
                sortText,
                q => q.Include(x => x.Product).ThenInclude(p => p.Images),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<DealCountdown?> GetDealCountdownWithProductById(Guid id, CancellationToken cancellationToken)
        {
            return await _context.Set<DealCountdown>()
                .Include(d => d.Product)
                .ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        public async Task<(int Total, int Live, int Upcoming, int Expired)> GetDealStatsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var total = await GetCountAsync(null, cancellationToken);
            var live = await GetCountAsync(x => x.IsActive && (!x.StartDate.HasValue || x.StartDate <= now) && x.EndDate >= now, cancellationToken);
            var upcoming = await GetCountAsync(x => x.IsActive && x.StartDate.HasValue && x.StartDate > now, cancellationToken);
            var expired = await GetCountAsync(x => x.EndDate < now, cancellationToken);

            return (total, live, upcoming, expired);
        }

        public async Task<DealCountdown?> GetActiveDealOfTheWeekAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var liveDeal = await _context.Set<DealCountdown>()
                .AsNoTracking()
                .Where(x => x.IsActive && (!x.StartDate.HasValue || x.StartDate <= now) && x.EndDate >= now)
                .Include(x => x.Product)
                .ThenInclude(p => p.Images)
                .OrderBy(x => x.EndDate)
                .FirstOrDefaultAsync(cancellationToken);

            if (liveDeal != null)
                return liveDeal;

            // Fallback to any active deal (e.g. upcoming or most recently updated)
            return await _context.Set<DealCountdown>()
                .AsNoTracking()
                .Where(x => x.IsActive)
                .Include(x => x.Product)
                .ThenInclude(p => p.Images)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}

