using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Queries
{
    public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, Category?>
    {
        private readonly IApplicationUnitOfWork _applicationUnitOfWork;

        public GetCategoryByIdQueryHandler(IApplicationUnitOfWork applicationUnitOfWork)
        {
            _applicationUnitOfWork = applicationUnitOfWork;
        }

        public async Task<Category?> Handle(GetCategoryByIdQuery query, CancellationToken cancellationToken)
        {
            return await _applicationUnitOfWork.CategoryRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}
