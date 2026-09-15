using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Queries
{
    public class GetHeroSlideByIdQueryHandler : IQueryHandler<GetHeroSlideByIdQuery, HeroSlide?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetHeroSlideByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HeroSlide?> Handle(GetHeroSlideByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.HeroSlideRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

