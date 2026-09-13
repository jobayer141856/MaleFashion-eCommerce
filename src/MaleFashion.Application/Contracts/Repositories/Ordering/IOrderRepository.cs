using MaleFashion.Application.Features.Ordering.DTOs;
using MaleFashion.Application.Features.Ordering.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Ordering;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Ordering
{
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<(IList<Order> Items, int Total, int TotalDisplay)> GetPagedOrders(GetAllOrdersByPagingQuery query, CancellationToken cancellationToken);
        Task<Order?> GetOrderWithDetailsById(Guid id, CancellationToken cancellationToken);
        Task<OrderStatsDto> GetOrderStatsAsync(CancellationToken cancellationToken);
    }
}

