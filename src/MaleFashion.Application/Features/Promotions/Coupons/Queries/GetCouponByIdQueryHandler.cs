using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.Coupons.Queries
{
    public class GetCouponByIdQueryHandler : IQueryHandler<GetCouponByIdQuery, Coupon?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetCouponByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Coupon?> Handle(GetCouponByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CouponRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

