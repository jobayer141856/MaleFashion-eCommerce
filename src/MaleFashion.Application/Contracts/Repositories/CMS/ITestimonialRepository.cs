using MaleFashion.Application.Features.CMS.Testimonials.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.CMS
{
    public interface ITestimonialRepository : IRepository<Testimonial, Guid>
    {
        Task<(IList<Testimonial>, int, int)> GetPagedTestimonials(GetAllTestimonialsByPagingQuery query, CancellationToken cancellationToken);
    }
}

