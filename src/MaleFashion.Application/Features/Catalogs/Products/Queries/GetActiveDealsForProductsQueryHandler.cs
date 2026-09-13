using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetActiveDealsForProductsQueryHandler : IQueryHandler<GetActiveDealsForProductsQuery, Dictionary<Guid, ProductActiveDealDto>>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetActiveDealsForProductsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Dictionary<Guid, ProductActiveDealDto>> Handle(GetActiveDealsForProductsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.GetActiveDealsForProductIdsAsync(query.ProductIds, cancellationToken);
        }
    }
}

