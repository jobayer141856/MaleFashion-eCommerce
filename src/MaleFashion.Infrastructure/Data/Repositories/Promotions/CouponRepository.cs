using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Features.Promotions.Coupons.Queries;
using MaleFashion.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Promotions
{
    public class CouponRepository : Repository<Coupon, Guid>, ICouponRepository
    {
        private readonly ApplicationDbContext _context;

        public CouponRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Coupon>, int, int)> GetPagedCoupons(GetAllCouponsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Code.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<bool> IsDuplicateCode(string code, Guid? id, CancellationToken cancellationToken)
        {
            var normalized = code.Trim().ToUpper();
            if (!id.HasValue)
                return (await GetCountAsync(x => x.Code.ToUpper() == normalized, cancellationToken)) > 0;
            else
                return (await GetCountAsync(x => x.Code.ToUpper() == normalized && x.Id != id.Value, cancellationToken)) > 0;
        }

        public async Task<(int Total, int Active, int Expired, int TotalUsed)> GetCouponStatsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var total = await GetCountAsync(null, cancellationToken);
            var active = await GetCountAsync(x => x.IsActive && x.ExpirationDate >= now, cancellationToken);
            var expired = await GetCountAsync(x => x.ExpirationDate < now, cancellationToken);
            var totalUsed = await _context.Set<Coupon>().SumAsync(x => x.UsedCount, cancellationToken);

            return (total, active, expired, totalUsed);
        }
    }
}

