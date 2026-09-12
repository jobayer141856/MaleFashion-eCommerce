using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.Partners.Queries;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.CMS
{
    public class PartnerClientRepository : Repository<PartnerClient, Guid>, IPartnerClientRepository
    {
        private readonly ApplicationDbContext _context;

        public PartnerClientRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<PartnerClient>, int, int)> GetPagedPartners(GetAllPartnersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.ClientName.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }
    }
}

