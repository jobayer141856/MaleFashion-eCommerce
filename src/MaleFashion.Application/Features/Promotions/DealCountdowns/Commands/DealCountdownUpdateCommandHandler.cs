using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownUpdateCommandHandler : ICommandHandler<DealCountdownUpdateCommand, DealCountdown>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public DealCountdownUpdateCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DealCountdown> Handle(DealCountdownUpdateCommand command, CancellationToken cancellationToken)
        {
            var deal = await _unitOfWork.DealCountdownRepository.GetByIdAsync(command.Id, cancellationToken);
            if (deal == null)
                throw new NotFoundException($"Deal countdown with ID '{command.Id}' not found.");

            var product = await _unitOfWork.ProductRepository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product == null)
                throw new NotFoundException($"Product with ID '{command.ProductId}' not found.");

            if (command.SalePrice <= 0)
                throw new InvalidOperationException("Flash deal sale price must be greater than zero.");

            if (product.BasePrice != null && command.SalePrice >= product.BasePrice.Amount)
                throw new InvalidOperationException($"Flash deal sale price ({command.SalePrice}) must be lower than product base price ({product.BasePrice.Amount} {product.BasePrice.Currency}).");

            if (command.StartDate.HasValue && command.EndDate <= command.StartDate.Value)
                throw new InvalidOperationException("Flash deal end date must be later than start date.");

            var currency = !string.IsNullOrWhiteSpace(command.Currency)
                ? command.Currency
                : (product.BasePrice?.Currency ?? "BDT");

            deal.ProductId = command.ProductId;
            deal.DealTitle = command.DealTitle.Trim();
            deal.SalePrice = new Money(command.SalePrice, currency);
            deal.StartDate = command.StartDate;
            deal.EndDate = command.EndDate;
            deal.IsActive = command.IsActive;
            deal.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.DealCountdownRepository.EditAsync(deal, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return deal;
        }
    }
}

