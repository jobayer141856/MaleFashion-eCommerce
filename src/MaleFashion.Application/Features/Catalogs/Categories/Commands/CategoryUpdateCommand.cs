using Cortex.Mediator.Commands;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Commands
{
    public class CategoryUpdateCommand : ICommand<Category>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; }
        public string? ImageName { get; set; }
    }
}