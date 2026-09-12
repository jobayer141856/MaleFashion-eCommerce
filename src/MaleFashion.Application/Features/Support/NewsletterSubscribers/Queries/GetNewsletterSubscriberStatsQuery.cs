using Cortex.Mediator.Queries;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries
{
    public class GetNewsletterSubscriberStatsQuery : IQuery<(int Total, int Active, int Inactive)>
    {
    }
}

