using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.CMS;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideUpdateCommandHandler : ICommandHandler<HeroSlideUpdateCommand, HeroSlide>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public HeroSlideUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HeroSlide> Handle(HeroSlideUpdateCommand command, CancellationToken cancellationToken)
        {
            var slide = await _unitOfWork.HeroSlideRepository.GetByIdAsync(command.Id, cancellationToken);
            if (slide == null)
                throw new NotFoundException($"Hero slide with ID '{command.Id}' not found.");

            slide.Subtitle = command.Subtitle;
            slide.Title = command.Title;
            slide.Description = command.Description;
            if (!string.IsNullOrEmpty(command.ImageUrl))
            {
                slide.ImageUrl = command.ImageUrl;
            }
            slide.ButtonText = string.IsNullOrWhiteSpace(command.ButtonText) ? "Shop now" : command.ButtonText;
            slide.TargetUrl = string.IsNullOrWhiteSpace(command.TargetUrl) ? "/Shop" : command.TargetUrl;
            slide.DisplayOrder = command.DisplayOrder;
            slide.IsActive = command.IsActive;
            slide.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.HeroSlideRepository.EditAsync(slide, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return slide;
        }
    }
}

