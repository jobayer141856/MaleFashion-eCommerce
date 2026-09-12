using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

