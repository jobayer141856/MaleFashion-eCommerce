using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Support.StoreLocations.Queries
{
    public class GetAllStoreLocationsByPagingQuery : IQuery<(IList<StoreLocation>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
    }
}

