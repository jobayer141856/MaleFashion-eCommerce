using System;
using System.Collections.Generic;
using System.Text;
using Cortex.Mediator.Commands;

namespace MaleFashion.Application.Features.Catalogs.Brands.Commands
{
    public class BrandDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
