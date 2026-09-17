using MaleFashion.Application.Features.Carts.DTOs;
using MaleFashion.Application.Features.Carts.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Carts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Carts
{
    public interface ICartRepository : IRepository<Cart, Guid>
    {
        Task<(IList<Cart> Items, int Total, int TotalDisplay)> GetPagedActiveCarts(GetAllActiveCartsByPagingQuery query, CancellationToken cancellationToken);
        Task<Cart?> GetCartWithDetailsById(Guid id, CancellationToken cancellationToken);
        Task<Cart?> GetActiveCartByCustomerOrSessionAsync(Guid? customerId, string? sessionId, CancellationToken cancellationToken);
        Task<ActiveCartStatsDto> GetActiveCartStatsAsync(CancellationToken cancellationToken);
        Task<int> ClearStaleCartsAsync(int olderThanDays, CancellationToken cancellationToken);
    }
}

