using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Support
{
    public class NewsletterSubscriberRepository : Repository<NewsletterSubscriber, Guid>, INewsletterSubscriberRepository
    {
        private readonly ApplicationDbContext _context;

        public NewsletterSubscriberRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<NewsletterSubscriber>, int, int)> GetPagedSubscribers(GetAllNewsletterSubscribersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Email.Contains(query.SearchText),
                query.SortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        {
            var normalizedEmail = email.Trim().ToLower();
            var list = await GetAsync(x => x.Email.ToLower() == normalizedEmail, null, cancellationToken);
            return list.FirstOrDefault();
        }

        public async Task<(int Total, int Active, int Inactive)> GetSubscriberCountsAsync(CancellationToken cancellationToken)
        {
            var total = await GetCountAsync(null, cancellationToken);
            var active = await GetCountAsync(x => x.IsActive, cancellationToken);
            return (total, active, total - active);
        }
    }
}

