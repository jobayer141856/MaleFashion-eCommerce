using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductVariantQuickUpdateStockCommand : ICommand<bool>
    {
        public Guid VariantId { get; set; }
        public int NewStockQuantity { get; set; }
    }
}

