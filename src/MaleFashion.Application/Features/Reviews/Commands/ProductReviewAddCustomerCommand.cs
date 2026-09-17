using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Reviews.Commands
{
    public class ProductReviewAddCustomerCommand : ICommand<(bool Success, string Message)>
    {
        public Guid ProductId { get; set; }
        public Guid? CustomerId { get; set; }
        public string ReviewerName { get; set; } = string.Empty;
        public string ReviewerEmail { get; set; } = string.Empty;
        public int Rating { get; set; } = 5;
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }
}

