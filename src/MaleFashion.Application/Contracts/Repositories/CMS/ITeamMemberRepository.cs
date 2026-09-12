using MaleFashion.Application.Features.CMS.TeamMembers.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.CMS
{
    public interface ITeamMemberRepository : IRepository<TeamMember, Guid>
    {
        Task<(IList<TeamMember>, int, int)> GetPagedTeamMembers(GetAllTeamMembersByPagingQuery query, CancellationToken cancellationToken);
    }
}

