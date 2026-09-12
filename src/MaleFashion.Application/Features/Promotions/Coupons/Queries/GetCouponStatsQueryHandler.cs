using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Queries
{
    public class GetCouponStatsQueryHandler : IQueryHandler<GetCouponStatsQuery, (int Total, int Active, int Expired, int TotalUsed)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetCouponStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(int Total, int Active, int Expired, int TotalUsed)> Handle(GetCouponStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CouponRepository.GetCouponStatsAsync(cancellationToken);
        }
    }
}

