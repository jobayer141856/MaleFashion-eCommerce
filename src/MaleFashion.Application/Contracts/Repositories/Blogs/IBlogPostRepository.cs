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
    public interface IBlogPostRepository : IRepository<BlogPost, Guid>
    {
        Task<(IList<BlogPost>, int, int)> GetPagedBlogPosts(GetAllBlogPostsByPagingQuery query, CancellationToken cancellationToken);
        Task<BlogPost?> GetBlogPostDetailsById(Guid id, CancellationToken cancellationToken);
        Task<(IList<BlogPost>, int)> GetPublishedBlogPosts(int pageIndex, int pageSize, string? tag, string? search, CancellationToken cancellationToken);
        Task<(BlogPost? PreviousPost, BlogPost? NextPost)> GetAdjacentPosts(DateTime publishedDate, Guid currentPostId, CancellationToken cancellationToken);
    }
}