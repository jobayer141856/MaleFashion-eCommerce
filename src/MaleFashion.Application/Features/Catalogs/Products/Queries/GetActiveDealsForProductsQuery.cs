using Cortex.Mediator.Queries;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetActiveDealsForProductsQuery : IQuery<Dictionary<Guid, ProductActiveDealDto>>
    {
        public List<Guid> ProductIds { get; set; } = new List<Guid>();
    }
}

