using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Catalogs.Products.Queries
{
    public class GetAllProductsByPagingQuery : DataTables, IQuery<(IList<Product>, int, int)>
    {
        public string? SearchText { get; set; }
        public string? SortText { get; set; }
    }
}

