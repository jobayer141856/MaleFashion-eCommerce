using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Support.StoreLocations.Commands
{
    public class StoreLocationDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

