using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetProductVariantsWithStockQueryHandler : IQueryHandler<GetProductVariantsWithStockQuery, IList<ProductVariantStockDto>>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetProductVariantsWithStockQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<ProductVariantStockDto>> Handle(GetProductVariantsWithStockQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.GetVariantsByProductIdAsync(query.ProductId, cancellationToken);
        }
    }
}

