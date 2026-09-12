using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.StoreLocations.Queries;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Support
{
    public class StoreLocationRepository : Repository<StoreLocation, Guid>, IStoreLocationRepository
    {
        private readonly ApplicationDbContext _context;

        public StoreLocationRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<StoreLocation>, int, int)> GetPagedStoreLocations(GetAllStoreLocationsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Title.Contains(query.SearchText) || x.Address.Contains(query.SearchText) || x.PhoneNumber.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<IList<StoreLocation>> GetActiveStoreLocations(CancellationToken cancellationToken)
        {
            return await GetAsync(
                x => x.IsActive,
                q => q.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Title),
                null,
                true,
                cancellationToken);
        }

        public async Task<bool> IsDuplicateTitle(string title, Guid? id, CancellationToken cancellationToken)
        {
            if (!id.HasValue)
                return (await GetCountAsync(x => x.Title == title, cancellationToken)) > 0;
            else
                return (await GetCountAsync(x => x.Title == title && x.Id != id.Value, cancellationToken)) > 0;
        }
    }
}

