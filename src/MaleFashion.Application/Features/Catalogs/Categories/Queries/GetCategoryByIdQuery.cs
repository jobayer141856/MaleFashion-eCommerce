using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Queries
{
    public class GetCategoryByIdQuery : IQuery<Category?>
    {
        public Guid Id { get; set; }
    }
}
