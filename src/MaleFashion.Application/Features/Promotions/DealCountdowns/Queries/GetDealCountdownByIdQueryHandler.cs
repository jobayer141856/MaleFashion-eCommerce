using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Promotions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetDealCountdownByIdQueryHandler : IQueryHandler<GetDealCountdownByIdQuery, DealCountdown?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetDealCountdownByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DealCountdown?> Handle(GetDealCountdownByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.DealCountdownRepository.GetDealCountdownWithProductById(query.Id, cancellationToken);
        }
    }
}

