using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Support;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries
{
    public class GetAllNewsletterSubscribersByPagingQuery : IQuery<(IList<NewsletterSubscriber>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
    }
}

