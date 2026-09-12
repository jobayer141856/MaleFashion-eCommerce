using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Domain.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands
{
    public class NewsletterSubscribeCommandHandler : ICommandHandler<NewsletterSubscribeCommand, NewsletterSubscribeResult>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public NewsletterSubscribeCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<NewsletterSubscribeResult> Handle(NewsletterSubscribeCommand command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(command.Email))
            {
                return new NewsletterSubscribeResult
                {
                    Success = false,
                    Message = "A valid email address is required."
                };
            }

            var normalizedEmail = command.Email.Trim().ToLowerInvariant();
            var existing = await _unitOfWork.NewsletterSubscriberRepository.GetByEmailAsync(normalizedEmail, cancellationToken);

            if (existing != null)
            {
                if (existing.IsActive)
                {
                    return new NewsletterSubscribeResult
                    {
                        Success = true,
                        Message = "You are already subscribed to our newsletter."
                    };
                }

                // Reactivate inactive subscriber
                existing.IsActive = true;
                existing.SubscribedAt = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;

                await _unitOfWork.NewsletterSubscriberRepository.EditAsync(existing, cancellationToken);
                await _unitOfWork.SaveAsync(cancellationToken);

                return new NewsletterSubscribeResult
                {
                    Success = true,
                    IsReactivated = true,
                    Message = "Welcome back! Your newsletter subscription has been reactivated."
                };
            }

            // Create new subscriber
            var subscriber = new NewsletterSubscriber
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                Email = normalizedEmail,
                SubscribedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.NewsletterSubscriberRepository.AddAsync(subscriber, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return new NewsletterSubscribeResult
            {
                Success = true,
                Message = "Thank you for subscribing to our newsletter!"
            };
        }
    }
}

