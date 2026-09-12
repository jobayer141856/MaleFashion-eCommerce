using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries
{
    public class GetNewsletterSubscriberStatsQueryHandler : IQueryHandler<GetNewsletterSubscriberStatsQuery, (int Total, int Active, int Inactive)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetNewsletterSubscriberStatsQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(int Total, int Active, int Inactive)> Handle(GetNewsletterSubscriberStatsQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.NewsletterSubscriberRepository.GetSubscriberCountsAsync(cancellationToken);
        }
    }
}

