using Cortex.Mediator.Queries;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetDealCountdownStatsQuery : IQuery<(int Total, int Live, int Upcoming, int Expired)>
    {
    }
}

