using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.Testimonials.Commands
{
    public class TestimonialUpdateCommandHandler : ICommandHandler<TestimonialUpdateCommand, Testimonial>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public TestimonialUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Testimonial> Handle(TestimonialUpdateCommand command, CancellationToken cancellationToken)
        {
            var testimonial = await _unitOfWork.TestimonialRepository.GetByIdAsync(command.Id, cancellationToken);
            if (testimonial == null)
                throw new NotFoundException($"Testimonial with ID '{command.Id}' not found.");

            testimonial.AuthorName = command.AuthorName;
            testimonial.AuthorProfession = command.AuthorProfession;
            testimonial.Quote = command.Quote;
            if (!string.IsNullOrEmpty(command.AvatarUrl))
            {
                testimonial.AvatarUrl = command.AvatarUrl;
            }
            testimonial.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.TestimonialRepository.EditAsync(testimonial, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return testimonial;
        }
    }
}

