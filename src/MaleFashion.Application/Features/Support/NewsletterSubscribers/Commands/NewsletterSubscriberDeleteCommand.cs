using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands
{
    public class NewsletterSubscriberDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

