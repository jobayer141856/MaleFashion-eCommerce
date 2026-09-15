using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Contracts;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Promotions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static MaleFashion.Domain.Contracts.IRepository;

namespace MaleFashion.Application.Contracts.Repositories.Catalogs
{
    public interface IProductRepository : IRepository<Product, Guid>
    {
        Task<(IList<Product>, int, int)> GetPagedProducts(GetAllProductsByPagingQuery query, CancellationToken cancellationToken);
        Task<Product?> GetProductDetailsById(Guid id, CancellationToken cancellationToken);
        Task<bool> IsDuplicateSku(string sku, Guid? id, CancellationToken cancellationToken);
        Task<ProductCatalogStatsDto> GetCatalogStatsAsync(CancellationToken cancellationToken);
        Task<IList<ProductVariantStockDto>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken);
        Task<bool> UpdateVariantStockAsync(Guid variantId, int newQuantity, CancellationToken cancellationToken);
        Task<Dictionary<Guid, ProductActiveDealDto>> GetActiveDealsForProductIdsAsync(List<Guid> productIds, CancellationToken cancellationToken);
        Task<List<Coupon>> GetActiveStoreCouponsAsync(CancellationToken cancellationToken);
        Task<IList<Product>> GetFeaturedHomeProductsAsync(int take, CancellationToken cancellationToken);
    }
}

