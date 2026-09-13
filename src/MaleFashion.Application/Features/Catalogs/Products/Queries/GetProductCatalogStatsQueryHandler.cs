using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetProductCatalogStatsQueryHandler : IQueryHandler<GetProductCatalogStatsQuery, ProductCatalogStatsDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetProductCatalogStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ProductCatalogStatsDto> Handle(GetProductCatalogStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.GetCatalogStatsAsync(cancellationToken);
        }
    }
}

