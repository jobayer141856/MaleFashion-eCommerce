using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.CMS.Testimonials.Commands
{
    public class TestimonialDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

