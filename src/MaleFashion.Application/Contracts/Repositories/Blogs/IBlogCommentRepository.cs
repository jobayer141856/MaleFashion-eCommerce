using MaleFashion.Application.Features.Blogs.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Blogs;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Blogs
{
    public interface IBlogCommentRepository : IRepository<BlogComment, Guid>
    {
        Task<(IList<BlogComment>, int, int)> GetPagedComments(GetAllBlogCommentsByPagingQuery query, CancellationToken cancellationToken);
    }
}