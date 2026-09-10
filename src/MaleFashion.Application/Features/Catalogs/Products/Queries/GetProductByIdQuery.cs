using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetProductByIdQuery : IQuery<Product?>
    {
        public Guid Id { get; set; }
    }
}

