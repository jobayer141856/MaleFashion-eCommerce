using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Support;
using System;

namespace MaleFashion.Application.Features.Support.StoreLocations.Queries
{
    public class GetStoreLocationByIdQuery : IQuery<StoreLocation?>
    {
        public Guid Id { get; set; }
    }
}

