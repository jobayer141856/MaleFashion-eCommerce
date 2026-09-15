using MaleFashion.Application.Features.Promotions.DealCountdowns.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Promotions
{
    public interface IDealCountdownRepository : IRepository<DealCountdown, Guid>
    {
        Task<(IList<DealCountdown>, int, int)> GetPagedDealCountdowns(GetAllDealCountdownsByPagingQuery query, CancellationToken cancellationToken);
        Task<DealCountdown?> GetDealCountdownWithProductById(Guid id, CancellationToken cancellationToken);
        Task<(int Total, int Live, int Upcoming, int Expired)> GetDealStatsAsync(CancellationToken cancellationToken);
        Task<DealCountdown?> GetActiveDealOfTheWeekAsync(CancellationToken cancellationToken);
    }
}

