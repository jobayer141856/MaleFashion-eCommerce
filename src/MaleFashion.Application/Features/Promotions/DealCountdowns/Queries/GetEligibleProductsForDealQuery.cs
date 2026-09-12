using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Queries
{
    public class GetEligibleProductsForDealQuery : IQuery<IList<Product>>
    {
    }
}

