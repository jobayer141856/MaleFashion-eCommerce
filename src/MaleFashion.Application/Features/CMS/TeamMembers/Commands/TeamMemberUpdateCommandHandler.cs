using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Commands
{
    public class TeamMemberUpdateCommandHandler : ICommandHandler<TeamMemberUpdateCommand, TeamMember>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public TeamMemberUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TeamMember> Handle(TeamMemberUpdateCommand command, CancellationToken cancellationToken)
        {
            var member = await _unitOfWork.TeamMemberRepository.GetByIdAsync(command.Id, cancellationToken);
            if (member == null)
                throw new NotFoundException($"Team member with ID '{command.Id}' not found.");

            member.Name = command.Name;
            member.Role = command.Role;
            if (!string.IsNullOrEmpty(command.PhotoUrl))
            {
                member.PhotoUrl = command.PhotoUrl;
            }
            member.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TeamMemberRepository.EditAsync(member, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return member;
        }
    }
}

