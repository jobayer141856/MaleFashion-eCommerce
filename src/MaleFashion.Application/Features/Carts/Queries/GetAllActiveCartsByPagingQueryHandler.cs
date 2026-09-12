using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Carts;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetAllActiveCartsByPagingQueryHandler : IQueryHandler<GetAllActiveCartsByPagingQuery, (IList<Cart>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllActiveCartsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<Cart>, int, int)> Handle(GetAllActiveCartsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CartRepository.GetPagedActiveCarts(query, cancellationToken);
        }
    }
}

