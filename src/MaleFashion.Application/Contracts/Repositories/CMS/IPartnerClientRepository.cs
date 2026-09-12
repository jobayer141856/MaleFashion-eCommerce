using MaleFashion.Application.Features.CMS.Partners.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.CMS
{
    public interface IPartnerClientRepository : IRepository<PartnerClient, Guid>
    {
        Task<(IList<PartnerClient>, int, int)> GetPagedPartners(GetAllPartnersByPagingQuery query, CancellationToken cancellationToken);
    }
}

