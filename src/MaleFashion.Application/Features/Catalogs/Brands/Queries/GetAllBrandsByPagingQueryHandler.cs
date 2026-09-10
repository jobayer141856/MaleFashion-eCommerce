using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Queries
{
    public class GetAllBrandsByPagingQueryHandler : IQueryHandler<GetAllBrandsByPagingQuery, (IList<Brand>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public GetAllBrandsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<(IList<Brand>, int, int)> Handle(GetAllBrandsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.BrandRepository.GetPagedBrands(query, cancellationToken);
        }
    }
}
