using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.ContactInquiries.Queries;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Support
{
    public class ContactInquiryRepository : Repository<ContactInquiry, Guid>, IContactInquiryRepository
    {
        private readonly ApplicationDbContext _context;

        public ContactInquiryRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<ContactInquiry>, int, int)> GetPagedContactInquiries(GetAllContactInquiriesByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Name.Contains(query.SearchText) || x.Email.Contains(query.SearchText) || x.Message.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }
    }
}

