using MaleFashion.Application.Features.Catalogs.Categories.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Catalogs
{
    public interface ICategoryRepository : IRepository<Category, Guid>
    {
        //Task AddAsync(Category category);
        Task<(IList<Category>, int, int)> GetPagedCategories(GetAllCategoriesByPagingQuery query, CancellationToken cancellationToken);
        Task<bool> IsDuplicateCategoryName(string name, Guid? Id, CancellationToken cancellationToken);
        Task<Dictionary<Guid, int>> GetProductCountsByCategoryAsync(CancellationToken cancellationToken = default);
    }
}

