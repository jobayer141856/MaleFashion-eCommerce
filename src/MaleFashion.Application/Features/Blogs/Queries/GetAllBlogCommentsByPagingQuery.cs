using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Blogs;
using MaleFashion.Domain.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Blogs.Queries
{
    public class GetAllBlogCommentsByPagingQuery : DataTables, IQuery<(IList<BlogComment>, int, int)>
    {
        public string? SearchText { get; set; }
        public string? SortText { get; set; }
        public bool? StatusFilter { get; set; } // null = All, true = Approved, false = Pending
    }
}
