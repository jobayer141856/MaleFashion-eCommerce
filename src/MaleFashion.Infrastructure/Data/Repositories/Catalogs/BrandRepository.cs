using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Brands.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data.Repositories.Catalogs
{
    public class BrandRepository : Repository<Brand, Guid>, IBrandRepository
    {
        private readonly ApplicationDbContext _context;

        public BrandRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Brand>, int, int)> GetPagedBrands(GetAllBrandsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Name.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }
        public async Task<bool> IsDuplicateBrandName(string name, Guid? id, CancellationToken cancellationToken)
        {
            if (!id.HasValue)
                return (await GetCountAsync(x => x.Name == name, cancellationToken)) > 0;
            else
                return (await GetCountAsync(x => x.Name == name && x.Id != id.Value, cancellationToken)) > 0;
        }
    }
}

