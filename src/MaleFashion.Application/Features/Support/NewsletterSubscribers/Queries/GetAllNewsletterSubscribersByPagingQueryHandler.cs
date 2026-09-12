using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Queries
{
    public class GetAllNewsletterSubscribersByPagingQueryHandler : IQueryHandler<GetAllNewsletterSubscribersByPagingQuery, (IList<NewsletterSubscriber>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllNewsletterSubscribersByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<NewsletterSubscriber>, int, int)> Handle(GetAllNewsletterSubscribersByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.NewsletterSubscriberRepository.GetPagedSubscribers(query, cancellationToken);
        }
    }
}

