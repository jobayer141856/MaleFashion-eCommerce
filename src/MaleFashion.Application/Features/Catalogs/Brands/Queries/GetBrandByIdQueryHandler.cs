using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Queries
{
    public class GetBrandByIdQueryHandler : IQueryHandler<GetBrandByIdQuery, Brand?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        public GetBrandByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<Brand?> Handle(GetBrandByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.BrandRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}
