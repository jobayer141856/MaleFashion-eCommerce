using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Reviews.Commands
{
    public class ProductReviewDeleteCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

