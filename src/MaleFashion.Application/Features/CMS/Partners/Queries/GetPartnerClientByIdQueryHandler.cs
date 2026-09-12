using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Partners.Queries
{
    public class GetPartnerClientByIdQueryHandler : IQueryHandler<GetPartnerClientByIdQuery, PartnerClient?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetPartnerClientByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<PartnerClient?> Handle(GetPartnerClientByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.PartnerClientRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

