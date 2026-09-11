using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Queries
{
    public class GetAllBlogPostsByPagingQuery : DataTables, IQuery<(IList<BlogPost>, int, int)>
    {
        public string? SearchText { get; set; }
        public string? SortText { get; set; }
    }
}
