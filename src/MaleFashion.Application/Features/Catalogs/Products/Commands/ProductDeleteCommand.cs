using Cortex.Mediator.Commands;
using System;

namespace MaleFashion.Application.Features.Catalogs.Products.Commands
{
    public class ProductDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}

