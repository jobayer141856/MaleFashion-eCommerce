using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.CMS.Partners.Commands
{
    public class PartnerClientDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

