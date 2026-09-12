using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands
{
    public class NewsletterSubscriberDeleteCommandHandler : ICommandHandler<NewsletterSubscriberDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public NewsletterSubscriberDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(NewsletterSubscriberDeleteCommand command, CancellationToken cancellationToken)
        {
            var subscriber = await _unitOfWork.NewsletterSubscriberRepository.GetByIdAsync(command.Id, cancellationToken);
            if (subscriber == null)
                throw new NotFoundException($"Newsletter subscriber with ID '{command.Id}' not found.");

            await _unitOfWork.NewsletterSubscriberRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

