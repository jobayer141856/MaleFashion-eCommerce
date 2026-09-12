using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.CMS;
using System;

namespace MaleFashion.Application.Features.CMS.Testimonials.Queries
{
    public class GetTestimonialByIdQuery : IQuery<Testimonial?>
    {
        public Guid Id { get; set; }
    }
}

