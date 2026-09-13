using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Ordering;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetAllOrdersByPagingQueryHandler : IQueryHandler<GetAllOrdersByPagingQuery, (IList<Order>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllOrdersByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<Order>, int, int)> Handle(GetAllOrdersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.OrderRepository.GetPagedOrders(query, cancellationToken);
        }
    }
}

