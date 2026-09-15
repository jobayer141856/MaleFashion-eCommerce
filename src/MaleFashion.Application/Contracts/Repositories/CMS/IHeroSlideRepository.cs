using MaleFashion.Application.Features.CMS.HeroSlides.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.CMS
{
    public interface IHeroSlideRepository : IRepository<HeroSlide, Guid>
    {
        Task<IList<HeroSlide>> GetActiveSlidesAsync(CancellationToken cancellationToken = default);
        Task<(IList<HeroSlide>, int, int)> GetPagedHeroSlides(GetAllHeroSlidesByPagingQuery query, CancellationToken cancellationToken);
        Task<bool> ToggleStatusAsync(Guid id, CancellationToken cancellationToken);
    }
}

