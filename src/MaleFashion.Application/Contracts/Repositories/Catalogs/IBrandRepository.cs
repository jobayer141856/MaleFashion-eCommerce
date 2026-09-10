using MaleFashion.Application.Features.Catalogs.Brands.Queries;
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
        Task<(IList<Brand>, int, int)> GetPagedBrands(GetAllBrandsByPagingQuery query, CancellationToken cancellationToken);
        Task<bool> IsDuplicateBrandName(string name, Guid? id, CancellationToken cancellationToken);
    }
}

