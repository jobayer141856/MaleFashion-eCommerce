using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Partners.Queries
{
    public class GetAllPartnersByPagingQueryHandler : IQueryHandler<GetAllPartnersByPagingQuery, (IList<PartnerClient>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllPartnersByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<PartnerClient>, int, int)> Handle(GetAllPartnersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.PartnerClientRepository.GetPagedPartners(query, cancellationToken);
        }
    }
}

