using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Utilities;
using MaleFashion.Domain.ValueOfObjects;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Promotions.DealCountdowns.Commands
{
    public class DealCountdownAddCommandHandler : ICommandHandler<DealCountdownAddCommand, DealCountdown>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public DealCountdownAddCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DealCountdown> Handle(DealCountdownAddCommand command, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(command.ProductId, cancellationToken);
            if (product == null)
                throw new NotFoundException($"Product with ID '{command.ProductId}' not found.");

            if (command.SalePrice <= 0)
                throw new InvalidOperationException("Flash deal sale price must be greater than zero.");

            if (product.BasePrice != null && command.SalePrice >= product.BasePrice.Amount)
                throw new InvalidOperationException($"Flash deal sale price ({command.SalePrice}) must be lower than the product base price ({product.BasePrice.Amount} {product.BasePrice.Currency}).");

            if (command.StartDate.HasValue && command.EndDate <= command.StartDate.Value)
                throw new InvalidOperationException("Flash deal end date must be later than start date.");

            var currency = !string.IsNullOrWhiteSpace(command.Currency)
                ? command.Currency
                : (product.BasePrice?.Currency ?? "BDT");

            var deal = new DealCountdown
            {
                Id = IdentityGenerator.NewSequentialGuid(),
                ProductId = command.ProductId,
                DealTitle = command.DealTitle.Trim(),
                SalePrice = new Money(command.SalePrice, currency),
                StartDate = command.StartDate,
                EndDate = command.EndDate,
                IsActive = command.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.DealCountdownRepository.AddAsync(deal, cancellationToken);
            await _unitOfWork.SaveAsync(cancellationToken);

            return deal;
        }
    }
}

