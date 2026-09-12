using Cortex.Mediator.Queries;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Testimonials.Queries
{
    public class GetAllTestimonialsByPagingQueryHandler : IQueryHandler<GetAllTestimonialsByPagingQuery, (IList<Testimonial>, int, int)>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public GetAllTestimonialsByPagingQueryHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<(IList<Testimonial>, int, int)> Handle(GetAllTestimonialsByPagingQuery query, CancellationToken cancellationToken)
        {
            return await _unitOfWork.TestimonialRepository.GetPagedTestimonials(query, cancellationToken);
        }
    }
}

