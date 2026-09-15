using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideUpdateCommand : ICommand<HeroSlide>
    {
        public Guid Id { get; set; }
        public string Subtitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string ButtonText { get; set; } = "Shop now";
        public string TargetUrl { get; set; } = "/Shop";
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

