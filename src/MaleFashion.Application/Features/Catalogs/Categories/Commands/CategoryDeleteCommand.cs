using Cortex.Mediator.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Commands
{
    public class CategoryDeleteCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
