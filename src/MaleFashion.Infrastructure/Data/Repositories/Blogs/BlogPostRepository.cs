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
    public class BlogPostRepository : Repository<BlogPost, Guid>, IBlogPostRepository
    {
        private readonly ApplicationDbContext _context;

        public BlogPostRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<BlogPost>, int, int)> GetPagedBlogPosts(GetAllBlogPostsByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "PublishedDate desc" : query.SortText;

            return await GetDynamicAsync(
                x => query.SearchText == null || x.Title.Contains(query.SearchText) || x.AuthorName.Contains(query.SearchText),
                sortText,
                x => x.Include(b => b.Tags).Include(b => b.Comments),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<BlogPost?> GetBlogPostDetailsById(Guid id, CancellationToken cancellationToken)
        {
            var list = await GetAsync(
                x => x.Id == id,
                x => x.Include(b => b.Tags)
                      .Include(b => b.Comments.OrderByDescending(c => c.CreatedAt)),
                cancellationToken);

            return list.FirstOrDefault();
        }

        public async Task<(IList<BlogPost>, int)> GetPublishedBlogPosts(int pageIndex, int pageSize, string? tag, string? search, CancellationToken cancellationToken)
        {
            var query = _context.Set<BlogPost>().AsNoTracking()
                .Where(b => b.IsPublished)
                .Include(b => b.Tags)
                .Include(b => b.Comments.Where(c => c.IsApproved))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(b => b.Title.Contains(term) || b.Content.Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(tag))
            {
                var t = tag.Trim();
                query = query.Where(b => b.Tags.Any(item => item.Name == t));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderByDescending(b => b.PublishedDate)
                                   .Skip((pageIndex - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync(cancellationToken);

            return (items, total);
        }

        public async Task<(BlogPost? PreviousPost, BlogPost? NextPost)> GetAdjacentPosts(DateTime publishedDate, Guid currentPostId, CancellationToken cancellationToken)
        {
            var prev = await _context.Set<BlogPost>().AsNoTracking()
                .Where(b => b.IsPublished && b.Id != currentPostId && b.PublishedDate <= publishedDate)
                .OrderByDescending(b => b.PublishedDate)
                .FirstOrDefaultAsync(cancellationToken);

            var next = await _context.Set<BlogPost>().AsNoTracking()
                .Where(b => b.IsPublished && b.Id != currentPostId && b.PublishedDate >= publishedDate)
                .OrderBy(b => b.PublishedDate)
                .FirstOrDefaultAsync(cancellationToken);

            return (prev, next);
        }
    }
}