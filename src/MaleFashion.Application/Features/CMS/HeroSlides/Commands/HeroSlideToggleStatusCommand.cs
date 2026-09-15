using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideToggleStatusCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
    }
}

