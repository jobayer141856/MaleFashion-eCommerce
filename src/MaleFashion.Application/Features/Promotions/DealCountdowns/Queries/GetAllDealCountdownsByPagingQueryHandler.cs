using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetAllDealCountdownsByPagingQueryHandler : IQueryHandler<GetAllDealCountdownsByPagingQuery, (IList<DealCountdown>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllDealCountdownsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<DealCountdown>, int, int)> Handle(GetAllDealCountdownsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.DealCountdownRepository.GetPagedDealCountdowns(query, cancellationToken);
        }
    }
}

