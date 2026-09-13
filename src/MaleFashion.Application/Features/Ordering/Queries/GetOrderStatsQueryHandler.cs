using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Ordering.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetOrderStatsQueryHandler : IQueryHandler<GetOrderStatsQuery, OrderStatsDto>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetOrderStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderStatsDto> Handle(GetOrderStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.OrderRepository.GetOrderStatsAsync(cancellationToken);
        }
    }
}

