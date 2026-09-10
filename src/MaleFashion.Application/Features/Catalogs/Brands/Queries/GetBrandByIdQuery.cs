using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Features.Catalogs.Brands.Queries
{
    public class GetBrandByIdQuery : IQuery<Brand?>
    {
        public Guid Id { get; set; }
    }
}
