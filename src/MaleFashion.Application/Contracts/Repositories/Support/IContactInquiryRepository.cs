using MaleFashion.Application.Features.Support.ContactInquiries.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Support
{
    public interface IContactInquiryRepository : IRepository<ContactInquiry, Guid>
    {
        Task<(IList<ContactInquiry>, int, int)> GetPagedContactInquiries(GetAllContactInquiriesByPagingQuery query, CancellationToken cancellationToken);
    }
}

