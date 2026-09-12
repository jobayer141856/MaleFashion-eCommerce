using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Carts;
using System;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetActiveCartByIdQuery : IQuery<Cart?>
    {
        public Guid Id { get; set; }

        public GetActiveCartByIdQuery(Guid id)
        {
            Id = id;
        }
    }
}

