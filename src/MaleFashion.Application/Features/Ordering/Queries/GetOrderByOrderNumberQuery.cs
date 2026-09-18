using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Ordering;
using System;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetOrderByOrderNumberQuery : IQuery<Order?>
    {
        public string OrderNumber { get; set; } = string.Empty;
    }
}

