using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Queries
{
    public class GetAllHeroSlidesByPagingQuery : IQuery<(IList<HeroSlide>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
    }
}

