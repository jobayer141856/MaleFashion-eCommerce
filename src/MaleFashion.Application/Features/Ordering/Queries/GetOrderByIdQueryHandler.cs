using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Ordering;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetOrderByIdQueryHandler : IQueryHandler<GetOrderByIdQuery, Order?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetOrderByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Order?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.OrderRepository.GetOrderWithDetailsById(query.Id, cancellationToken);
        }
    }
}

