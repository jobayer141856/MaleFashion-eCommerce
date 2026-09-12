using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Queries
{
    public class GetAllCouponsByPagingQueryHandler : IQueryHandler<GetAllCouponsByPagingQuery, (IList<Coupon>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllCouponsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<Coupon>, int, int)> Handle(GetAllCouponsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CouponRepository.GetPagedCoupons(query, cancellationToken);
        }
    }
}

