using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Queries
{
    public class GetAllCategoriesQueryHandler : IQueryHandler<GetAllCategoriesQuery, IList<Category>>
    {
        private readonly IApplicationUnitOfWork _applicationUnitOfWork;

        public GetAllCategoriesQueryHandler(IApplicationUnitOfWork applicationUnitOfWork)
        {
            _applicationUnitOfWork = applicationUnitOfWork;
        }

        public async Task<IList<Category>> Handle(GetAllCategoriesQuery query, CancellationToken cancellationToken)
        {
            var categories = await _applicationUnitOfWork.CategoryRepository.GetAllAsync(cancellationToken);

            if (query.ActiveOnly == true)
            {
                categories = categories
                    .Where(c => c.IsActive).ToList();
            }

            if (query.SearchText is not null)
            {
                categories = categories
                    .Where(c => c.Name.Contains(query.SearchText, StringComparison.OrdinalIgnoreCase)).ToList();
            }


            return categories;
        }
    }
}
