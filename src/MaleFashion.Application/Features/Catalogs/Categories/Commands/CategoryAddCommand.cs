using Cortex.Mediator.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using MaleFashion.Domain.Entities.Catalogs;

namespace MaleFashion.Application.Features.Catalogs.Categories.Commands
{
    public class CategoryAddCommand : ICommand<Category>
    {

        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? ImageName { get; set; }

    }
}
