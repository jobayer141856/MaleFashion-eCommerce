using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Carts;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Carts.Queries
{
    public class GetAllActiveCartsByPagingQuery : IQuery<(IList<Cart>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
        public string? CartFilter { get; set; } // "all", "active", "idle", "abandoned", "registered", "guest"
    }
}

