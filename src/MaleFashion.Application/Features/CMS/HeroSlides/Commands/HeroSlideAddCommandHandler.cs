using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideAddCommandHandler : ICommandHandler<HeroSlideAddCommand, HeroSlide>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public HeroSlideAddCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HeroSlide> Handle(HeroSlideAddCommand command, CancellationToken cancellationToken)
        {
            var slide = new HeroSlide
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                Subtitle = command.Subtitle,
                Title = command.Title,
                Description = command.Description,
                ImageUrl = command.ImageUrl,
                ButtonText = string.IsNullOrWhiteSpace(command.ButtonText) ? "Shop now" : command.ButtonText,
                TargetUrl = string.IsNullOrWhiteSpace(command.TargetUrl) ? "/Shop" : command.TargetUrl,
                DisplayOrder = command.DisplayOrder,
                IsActive = command.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HeroSlideRepository.AddAsync(slide, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return slide;
        }
    }
}

