using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

