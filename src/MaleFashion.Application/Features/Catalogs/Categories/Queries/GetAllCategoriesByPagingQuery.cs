using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Queries
{
    public class GetAllCategoriesByPagingQuery : IQuery<(IList<Category>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; }
        public string SortText { get; set; }
    }
}
