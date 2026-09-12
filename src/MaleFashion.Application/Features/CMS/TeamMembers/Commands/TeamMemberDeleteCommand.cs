using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Commands
{
    public class TeamMemberDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

