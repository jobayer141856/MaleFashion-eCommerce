using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.CMS.HeroSlides.Commands
{
    public class HeroSlideToggleStatusCommandHandler : ICommandHandler<HeroSlideToggleStatusCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public HeroSlideToggleStatusCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(HeroSlideToggleStatusCommand command, CancellationToken cancellationToken)
        {
            return await _unitOfWork.HeroSlideRepository.ToggleStatusAsync(command.Id, cancellationToken);
        }
    }
}

