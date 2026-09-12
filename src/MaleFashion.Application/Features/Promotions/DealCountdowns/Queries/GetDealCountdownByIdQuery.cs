using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Promotions;
using System;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetDealCountdownByIdQuery : IQuery<DealCountdown?>
    {
        public Guid Id { get; set; }
    }
}

