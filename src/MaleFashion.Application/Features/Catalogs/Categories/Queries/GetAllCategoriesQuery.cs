using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Categories.Queries
{
    public class GetAllCategoriesQuery : IQuery<IList<Category>>
    {
        public bool? ActiveOnly { get; set; }
        public string? SearchText { get; set; }
    }
}
