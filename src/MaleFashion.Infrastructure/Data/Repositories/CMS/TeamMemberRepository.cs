using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.TeamMembers.Queries;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.CMS
{
    public class TeamMemberRepository : Repository<TeamMember, Guid>, ITeamMemberRepository
    {
        private readonly ApplicationDbContext _context;

        public TeamMemberRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<TeamMember>, int, int)> GetPagedTeamMembers(GetAllTeamMembersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Name.Contains(query.SearchText) || x.Role.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }
    }
}

