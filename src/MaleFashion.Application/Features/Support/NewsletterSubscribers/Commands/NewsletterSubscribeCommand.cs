using Cortex.Mediator.Commands;

namespace MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands
{
    public class NewsletterSubscribeResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsReactivated { get; set; }
    }

    public class NewsletterSubscribeCommand : ICommand<NewsletterSubscribeResult>
    {
        public string Email { get; set; } = string.Empty;
    }
}

