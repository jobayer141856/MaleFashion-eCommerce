using MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Support
{
    public interface INewsletterSubscriberRepository : IRepository<NewsletterSubscriber, Guid>
    {
        Task<(IList<NewsletterSubscriber>, int, int)> GetPagedSubscribers(GetAllNewsletterSubscribersByPagingQuery query, CancellationToken cancellationToken);
        Task<NewsletterSubscriber?> GetByEmailAsync(string email, CancellationToken cancellationToken);
        Task<(int Total, int Active, int Inactive)> GetSubscriberCountsAsync(CancellationToken cancellationToken);
    }
}

