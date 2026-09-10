using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetAllProductsByPagingQueryHandler : IQueryHandler<GetAllProductsByPagingQuery, (IList<Product>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllProductsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<Product>, int, int)> Handle(GetAllProductsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.GetPagedProducts(query, cancellationToken);
        }
    }
}

