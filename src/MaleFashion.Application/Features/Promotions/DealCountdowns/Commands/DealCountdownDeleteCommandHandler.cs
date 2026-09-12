using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownDeleteCommandHandler : ICommandHandler<DealCountdownDeleteCommand>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public DealCountdownDeleteCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task Handle(DealCountdownDeleteCommand command, CancellationToken cancellationToken)
        {
            var deal = await _unitOfWork.DealCountdownRepository.GetByIdAsync(command.Id, cancellationToken);
            if (deal == null)
                throw new NotFoundException($"Deal countdown with ID '{command.Id}' not found.");

            await _unitOfWork.DealCountdownRepository.RemoveAsync(command.Id, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);
        }
    }
}

