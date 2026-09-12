using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Testimonials.Queries
{
    public class GetTestimonialByIdQueryHandler : IQueryHandler<GetTestimonialByIdQuery, Testimonial?>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetTestimonialByIdQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Testimonial?> Handle(GetTestimonialByIdQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.TestimonialRepository.GetByIdAsync(query.Id, cancellationToken);
        }
    }
}

