using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Carts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetActiveCartByIdQueryHandler : IQueryHandler<GetActiveCartByIdQuery, Cart?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetActiveCartByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Cart?> Handle(GetActiveCartByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.CartRepository.GetCartWithDetailsById(query.Id, cancellationToken);
        }
    }
}

