using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.Partners.Commands
{
    public class PartnerClientUpdateCommand : ICommand<PartnerClient>
    {
        public Guid Id { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }
}

