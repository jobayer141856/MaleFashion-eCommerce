using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.StoreLocations.Queries
{
    public class GetActiveStoreLocationsQueryHandler : IQueryHandler<GetActiveStoreLocationsQuery, IList<StoreLocation>>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetActiveStoreLocationsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IList<StoreLocation>> Handle(GetActiveStoreLocationsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.StoreLocationRepository.GetActiveStoreLocations(cancellationToken);
        }
    }
}

