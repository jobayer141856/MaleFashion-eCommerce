using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Utilities;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Testimonials.Commands
{
    public class TestimonialAddCommandHandler : ICommandHandler<TestimonialAddCommand, Testimonial>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public TestimonialAddCommandHandler(IApplicationUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Testimonial> Handle(TestimonialAddCommand command, CancellationToken cancellationToken)
        {
            var testimonial = _mapper.Map<Testimonial>(command);
            testimonial.Id = IdentityGenerator.NewSequentialGuid();
            testimonial.CreatedAt = DateTime.UtcNow;
            if (string.IsNullOrWhiteSpace(testimonial.AvatarUrl))
            {
                testimonial.AvatarUrl = "malefashion/img/about/testimonial-author.jpg";
            }

            await _unitOfWork.TestimonialRepository.AddAsync(testimonial, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return testimonial;
        }
    }
}

