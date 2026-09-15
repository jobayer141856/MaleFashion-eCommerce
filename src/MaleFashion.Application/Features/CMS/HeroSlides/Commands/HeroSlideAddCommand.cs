using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideAddCommand : ICommand<HeroSlide>
    {
        public string Subtitle { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string ButtonText { get; set; } = "Shop now";
        public string TargetUrl { get; set; } = "/Shop";
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

