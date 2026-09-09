using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using System;
using System.Collections.Generic;
using System.Text;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Catalogs
{
    public interface IBrandRepository : IRepository<Brand, Guid>
    {
    }
}

