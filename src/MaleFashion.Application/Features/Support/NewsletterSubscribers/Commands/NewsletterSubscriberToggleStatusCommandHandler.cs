using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands
{
    public class NewsletterSubscriberToggleStatusCommandHandler : ICommandHandler<NewsletterSubscriberToggleStatusCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public NewsletterSubscriberToggleStatusCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(NewsletterSubscriberToggleStatusCommand command, CancellationToken cancellationToken)
        {
            var subscriber = await _unitOfWork.NewsletterSubscriberRepository.GetByIdAsync(command.Id, cancellationToken);
            if (subscriber == null)
                throw new NotFoundException($"Newsletter subscriber with ID '{command.Id}' not found.");

            subscriber.IsActive = !subscriber.IsActive;
            subscriber.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.NewsletterSubscriberRepository.EditAsync(subscriber, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return subscriber.IsActive;
        }
    }
}

