using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownToggleStatusCommandHandler : ICommandHandler<DealCountdownToggleStatusCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public DealCountdownToggleStatusCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(DealCountdownToggleStatusCommand command, CancellationToken cancellationToken)
        {
            var deal = await _unitOfWork.DealCountdownRepository.GetByIdAsync(command.Id, cancellationToken);
            if (deal == null)
                throw new NotFoundException($"Deal countdown with ID '{command.Id}' not found.");

            deal.IsActive = !deal.IsActive;
            deal.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.DealCountdownRepository.EditAsync(deal, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return deal.IsActive;
        }
    }
}

