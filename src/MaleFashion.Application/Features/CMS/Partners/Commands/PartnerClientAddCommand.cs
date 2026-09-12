using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;

namespace MaleFashion.Application.Features.CMS.Partners.Commands
{
    public class PartnerClientAddCommand : ICommand<PartnerClient>
    {
        public string ClientName { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
    }
}

