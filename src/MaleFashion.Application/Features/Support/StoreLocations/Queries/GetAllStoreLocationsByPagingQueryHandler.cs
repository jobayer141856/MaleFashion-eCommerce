using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.StoreLocations.Queries
{
    public class GetAllStoreLocationsByPagingQueryHandler : IQueryHandler<GetAllStoreLocationsByPagingQuery, (IList<StoreLocation>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllStoreLocationsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<StoreLocation>, int, int)> Handle(GetAllStoreLocationsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.StoreLocationRepository.GetPagedStoreLocations(query, cancellationToken);
        }
    }
}

