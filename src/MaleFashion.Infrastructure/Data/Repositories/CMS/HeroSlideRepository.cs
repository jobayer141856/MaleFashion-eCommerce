using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Features.CMS.HeroSlides.Queries;
using MaleFashion.Domain.Entities.CMS;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.CMS
{
    public class HeroSlideRepository : Repository<HeroSlide, Guid>, IHeroSlideRepository
    {
        private readonly ApplicationDbContext _context;

        public HeroSlideRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<IList<HeroSlide>> GetActiveSlidesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<HeroSlide>()
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IList<HeroSlide>, int, int)> GetPagedHeroSlides(GetAllHeroSlidesByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) ? "DisplayOrder asc" : query.SortText;
            return await GetDynamicAsync(
                x => query.SearchText == null || x.Title.Contains(query.SearchText) || x.Subtitle.Contains(query.SearchText),
                sortText,
                null,
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken)
        {
            var slide = await _context.Set<HeroSlide>().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
            if (slide == null) return false;

            slide.IsActive = !slide.IsActive;
            slide.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            return slide.IsActive;
        }
    }
}

