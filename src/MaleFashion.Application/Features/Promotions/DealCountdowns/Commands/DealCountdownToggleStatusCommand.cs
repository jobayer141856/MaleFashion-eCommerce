using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownToggleStatusCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

