using Cortex.Mediator.Commands;
using MaleFashion.Application.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductVariantQuickUpdateStockCommandHandler : ICommandHandler<ProductVariantQuickUpdateStockCommand, bool>
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ProductVariantQuickUpdateStockCommandHandler(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(ProductVariantQuickUpdateStockCommand command, CancellationToken cancellationToken)
        {
            return await _unitOfWork.ProductRepository.UpdateVariantStockAsync(command.VariantId, command.NewStockQuantity, cancellationToken);
        }
    }
}

