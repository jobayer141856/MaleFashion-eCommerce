using MaleFashion.Application.Features.Support.StoreLocations.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Support
{
    public interface IStoreLocationRepository : IRepository<StoreLocation, Guid>
    {
        Task<(IList<StoreLocation>, int, int)> GetPagedStoreLocations(GetAllStoreLocationsByPagingQuery query, CancellationToken cancellationToken);
        Task<IList<StoreLocation>> GetActiveStoreLocations(CancellationToken cancellationToken);
        Task<bool> IsDuplicateTitle(string title, Guid? id, CancellationToken cancellationToken);
    }
}

