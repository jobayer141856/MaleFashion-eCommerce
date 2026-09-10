using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Queries
{
    public class GetAllBrandsQueryHandler : IQueryHandler<GetAllBrandsQuery, IList<Brand>>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public GetAllBrandsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IList<Brand>> Handle(GetAllBrandsQuery query, CancellationToken cancellationToken)
        {
            var brands = await _unitOfWork.BrandRepository.GetAllAsync(cancellationToken);
            if (query.ActiveOnly == true)
            {
                brands = brands.Where(b => b.IsActive).ToList();
            }
            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                brands = brands.Where(b => b.Name.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return brands;
        }
    }
}
