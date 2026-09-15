using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Queries
{
    public class GetHeroSlideByIdQuery : IQuery<HeroSlide?>
    {
        public Guid Id { get; set; }
    }
}

