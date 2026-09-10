using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Commands
{
    public class BrandUpdateCommand : ICommand<Brand>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
