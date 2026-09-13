using Cortex.Mediator.Queries;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetProductVariantsWithStockQuery : IQuery<IList<ProductVariantStockDto>>
    {
        public Guid ProductId { get; set; }
    }
}

