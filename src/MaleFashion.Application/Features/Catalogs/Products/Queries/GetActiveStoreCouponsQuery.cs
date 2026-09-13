using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Promotions;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetActiveStoreCouponsQuery : IQuery<List<Coupon>>
    {
    }
}

