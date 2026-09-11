using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Features.Blogs.Queries;
using MaleFashion.Domain.Entities.Blogs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Blogs
{
    public class BlogCommentRepository : Repository<BlogComment, Guid>, IBlogCommentRepository
    {
        private readonly ApplicationDbContext _context;
        public BlogCommentRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }
        

        public async Task<(IList<BlogComment>, int, int)> GetPagedComments(GetAllBlogCommentsByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "CreatedAt desc" : query.SortText;

            return await GetDynamicAsync(
                x => (query.SearchText == null || x.Name.Contains(query.SearchText) || x.CommentText.Contains(query.SearchText) || x.Email.Contains(query.SearchText)) &&
                     (!query.StatusFilter.HasValue || (query.StatusFilter.Value ? x.IsApproved : !x.IsApproved)),
                sortText,
                x => x.Include(c => c.BlogPost),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }
    }
}