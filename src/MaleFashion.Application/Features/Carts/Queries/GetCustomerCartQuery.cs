using Cortex.Mediator.Queries;
using MaleFashion.Application.Features.Carts.DTOs;
using System;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetCustomerCartQuery : IQuery<CustomerCartDto>
    {
        public Guid? CustomerId { get; set; }
        public string? SessionId { get; set; }
    }
}

