using Cortex.Mediator.Queries;

namespace MaleFashion.Application.Features.Promotions.Coupons.Queries
{
    public class GetCouponStatsQuery : IQuery<(int Total, int Active, int Expired, int TotalUsed)>
    {
    }
}

