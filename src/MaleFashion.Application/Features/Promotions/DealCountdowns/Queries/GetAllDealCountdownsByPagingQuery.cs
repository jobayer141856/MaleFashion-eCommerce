using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Promotions;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetAllDealCountdownsByPagingQuery : IQuery<(IList<DealCountdown>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
    }
}

