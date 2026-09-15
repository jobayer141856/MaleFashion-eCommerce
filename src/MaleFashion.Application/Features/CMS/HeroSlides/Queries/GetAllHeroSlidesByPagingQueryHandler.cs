using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Queries
{
    public class GetAllHeroSlidesByPagingQueryHandler : IQueryHandler<GetAllHeroSlidesByPagingQuery, (IList<HeroSlide>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllHeroSlidesByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<HeroSlide>, int, int)> Handle(GetAllHeroSlidesByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.HeroSlideRepository.GetPagedHeroSlides(query, cancellationToken);
        }
    }
}

