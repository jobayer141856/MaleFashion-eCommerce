using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Ordering;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetOrderByOrderNumberQueryHandler : IQueryHandler<GetOrderByOrderNumberQuery, Order?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetOrderByOrderNumberQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order?> Handle(GetOrderByOrderNumberQuery query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(query.OrderNumber))
                return null;

            return await _unitOfWork.OrderRepository.GetOrderByOrderNumberWithDetailsAsync(query.OrderNumber.Trim(), cancellationToken);
        }
    }
}

