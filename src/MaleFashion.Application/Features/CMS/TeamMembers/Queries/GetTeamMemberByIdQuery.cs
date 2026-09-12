using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.TeamMembers.Queries
{
    public class GetTeamMemberByIdQuery : IQuery<TeamMember?>
    {
        public Guid Id { get; set; }
    }
}

