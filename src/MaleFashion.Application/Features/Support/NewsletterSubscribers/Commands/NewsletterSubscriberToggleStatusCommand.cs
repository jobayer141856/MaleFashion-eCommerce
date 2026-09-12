using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands
{
    public class NewsletterSubscriberToggleStatusCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

