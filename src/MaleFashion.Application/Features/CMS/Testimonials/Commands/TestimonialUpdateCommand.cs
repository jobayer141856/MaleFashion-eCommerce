using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.Testimonials.Commands
{
    public class TestimonialUpdateCommand : ICommand<Testimonial>
    {
        public Guid Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorProfession { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Quote { get; set; } = string.Empty;
    }
}

