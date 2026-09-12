using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Commands
{
    public class TeamMemberAddCommandHandler : ICommandHandler<TeamMemberAddCommand, TeamMember>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TeamMemberAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<TeamMember> Handle(TeamMemberAddCommand command, CancellationToken cancellationToken)
        {
            var member = _mapper.Map<TeamMember>(command);
            member.Id = IdentityGenerator.NewSequentialGuid();
            member.CreatedAt = DateTime.UtcNow;

            await _unitOfWork.TeamMemberRepository.AddAsync(member, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return member;
        }
    }
}

