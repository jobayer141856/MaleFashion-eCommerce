using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Categories.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Catalogs
{
    public class CategoryRepository : Repository<Category, Guid>, ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Category>, int, int)> GetPagedCategories(GetAllCategoriesByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(x => query.SearchText == null || x.Name.Contains(query.SearchText),
                 query.SortText,
                 x => x.Include(c => c.ParentCategory),
                 query.PageIndex,
                 query.PageSize,
                 true,
                 cancellationToken);
        }

        public async Task<bool> IsDuplicateCategoryName(string name, Guid? id, CancellationToken cancellationToken)
        {
            if (!id.HasValue)
                return (await GetCountAsync(x => x.Name == name, cancellationToken)) > 0;
            else
                return (await GetCountAsync(x => x.Name == name && x.Id != id.Value, cancellationToken)) > 0;
        }
    }
}

