using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Infrastructure.Data.Repositories.Catalogs
{
    public class ProductRepository : Repository<Product, Guid>, IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepository(ApplicationDbContext dbContext) : base(dbContext)
        {
            _context = dbContext;
        }

        public async Task<(IList<Product>, int, int)> GetPagedProducts(GetAllProductsByPagingQuery query, CancellationToken cancellationToken)
        {
            var sortText = string.IsNullOrWhiteSpace(query.SortText) || query.SortText.Contains("Images")
                ? "Name asc"
                : query.SortText;

            return await GetDynamicAsync(
                x => query.SearchText == null || x.Name.Contains(query.SearchText) || x.Sku.Contains(query.SearchText),
                sortText,
                x => x.Include(p => p.Category)
                      .Include(p => p.Brand)
                      .Include(p => p.Images),
                query.PageIndex,
                query.PageSize,
                true,
                cancellationToken);
        }

        public async Task<Product?> GetProductDetailsById(Guid id, CancellationToken cancellationToken)
        {
            var list = await GetAsync(
                x => x.Id == id,
                x => x.Include(p => p.Category)
                      .Include(p => p.Brand)
                      .Include(p => p.Images)
                      .Include(p => p.Variants),
                cancellationToken);

            return list.FirstOrDefault();
        }

        public async Task<bool> IsDuplicateSku(string sku, Guid? id, CancellationToken cancellationToken)
        {
            if (!id.HasValue)
                return (await GetCountAsync(x => x.Sku == sku, cancellationToken)) > 0;
            else
                return (await GetCountAsync(x => x.Sku == sku && x.Id != id.Value, cancellationToken)) > 0;
        }
    }
}

