using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Ordering;
using System;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetOrderByIdQuery : IQuery<Order?>
    {
        public Guid Id { get; set; }

        public GetOrderByIdQuery(Guid id)
        {
            Id = id;
        }

        public GetOrderByIdQuery() { }
    }
}

