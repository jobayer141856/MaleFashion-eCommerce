using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Commands
{
    public class TeamMemberAddCommand : ICommand<TeamMember>
    {
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
    }
}

