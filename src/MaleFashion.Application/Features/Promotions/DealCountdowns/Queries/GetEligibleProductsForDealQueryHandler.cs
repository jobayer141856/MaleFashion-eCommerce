using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetEligibleProductsForDealQueryHandler : IQueryHandler<GetEligibleProductsForDealQuery, IList<Product>>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetEligibleProductsForDealQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<Product>> Handle(GetEligibleProductsForDealQuery query, CancellationToken cancellationToken)
        {
            var products = await _unitOfWork.ProductRepository.GetAllAsync(cancellationToken);
            return products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
        }
    }
}

