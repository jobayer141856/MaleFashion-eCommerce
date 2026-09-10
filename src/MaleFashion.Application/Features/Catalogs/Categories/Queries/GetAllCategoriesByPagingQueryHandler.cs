using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Queries
{
    public class GetAllCategoriesByPagingQueryHandler : IQueryHandler<GetAllCategoriesByPagingQuery, (IList<Category>, int, int)>
    {

        private readonly IApplicationUnitOfWork _applicationUnitOfWork;

        public GetAllCategoriesByPagingQueryHandler(IApplicationUnitOfWork applicationUnitOfWork)
        {
            _applicationUnitOfWork = applicationUnitOfWork;
        }

        public async Task<(IList<Category>, int, int)> Handle(GetAllCategoriesByPagingQuery query,
            CancellationToken cancellationToken)
        {
            return await _applicationUnitOfWork.CategoryRepository.GetPagedCategories(query, cancellationToken);
        }
    }
}
