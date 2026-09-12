using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetDealCountdownStatsQueryHandler : IQueryHandler<GetDealCountdownStatsQuery, (int Total, int Live, int Upcoming, int Expired)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetDealCountdownStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(int Total, int Live, int Upcoming, int Expired)> Handle(GetDealCountdownStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.DealCountdownRepository.GetDealStatsAsync(cancellationToken);
        }
    }
}

