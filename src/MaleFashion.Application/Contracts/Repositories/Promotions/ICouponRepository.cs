using MaleFashion.Application.Features.Promotions.Coupons.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Promotions
{
    public interface ICouponRepository : IRepository<Coupon, Guid>
    {
        Task<(IList<Coupon>, int, int)> GetPagedCoupons(GetAllCouponsByPagingQuery query, CancellationToken cancellationToken);
        Task<bool> IsDuplicateCode(string code, Guid? id, CancellationToken cancellationToken);
        Task<(int Total, int Active, int Expired, int TotalUsed)> GetCouponStatsAsync(CancellationToken cancellationToken);
    }
}

