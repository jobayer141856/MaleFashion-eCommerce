using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Queries
{
    public class GetAllTeamMembersByPagingQueryHandler : IQueryHandler<GetAllTeamMembersByPagingQuery, (IList<TeamMember>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllTeamMembersByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<TeamMember>, int, int)> Handle(GetAllTeamMembersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.TeamMemberRepository.GetPagedTeamMembers(query, cancellationToken);
        }
    }
}

