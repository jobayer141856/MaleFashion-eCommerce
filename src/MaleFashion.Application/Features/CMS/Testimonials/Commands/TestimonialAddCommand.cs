using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.CMS;

namespace MaleFashion.Application.Features.CMS.Testimonials.Commands
{
    public class TestimonialAddCommand : ICommand<Testimonial>
    {
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorProfession { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public string Quote { get; set; } = string.Empty;
    }
}

