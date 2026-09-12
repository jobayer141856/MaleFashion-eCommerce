using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Commands
{
    public class TeamMemberUpdateCommand : ICommand<TeamMember>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}

