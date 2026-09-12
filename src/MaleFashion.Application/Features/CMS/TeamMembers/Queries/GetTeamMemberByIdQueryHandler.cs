using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Queries
{
    public class GetTeamMemberByIdQueryHandler : IQueryHandler<GetTeamMemberByIdQuery, TeamMember?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetTeamMemberByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TeamMember?> Handle(GetTeamMemberByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.TeamMemberRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

