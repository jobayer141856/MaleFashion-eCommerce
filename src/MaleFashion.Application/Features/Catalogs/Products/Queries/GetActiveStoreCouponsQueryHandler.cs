using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetActiveStoreCouponsQueryHandler : IQueryHandler<GetActiveStoreCouponsQuery, List<Coupon>>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetActiveStoreCouponsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<Coupon>> Handle(GetActiveStoreCouponsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.GetActiveStoreCouponsAsync(cancellationToken);
        }
    }
}

