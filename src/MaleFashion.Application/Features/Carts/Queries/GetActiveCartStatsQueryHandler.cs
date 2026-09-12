using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Carts.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetActiveCartStatsQueryHandler : IQueryHandler<GetActiveCartStatsQuery, ActiveCartStatsDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetActiveCartStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ActiveCartStatsDto> Handle(GetActiveCartStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CartRepository.GetActiveCartStatsAsync(cancellationToken);
        }
    }
}

