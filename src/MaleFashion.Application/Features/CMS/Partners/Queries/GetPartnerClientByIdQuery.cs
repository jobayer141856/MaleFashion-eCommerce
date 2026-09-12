using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.Partners.Queries
{
    public class GetPartnerClientByIdQuery : IQuery<PartnerClient?>
    {
        public Guid Id { get; set; }
    }
}

