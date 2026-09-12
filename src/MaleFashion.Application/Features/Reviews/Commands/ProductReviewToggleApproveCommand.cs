using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Reviews.Commands
{
    public class ProductReviewToggleApproveCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

