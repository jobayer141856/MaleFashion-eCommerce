using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.StoreLocations.Queries
{
    public class GetStoreLocationByIdQueryHandler : IQueryHandler<GetStoreLocationByIdQuery, StoreLocation?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetStoreLocationByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<StoreLocation?> Handle(GetStoreLocationByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.StoreLocationRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

