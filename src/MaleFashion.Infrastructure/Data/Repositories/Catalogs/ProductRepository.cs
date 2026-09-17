using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Promotions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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

            if (sortText.Contains("BasePrice") && !sortText.Contains("BasePrice.Amount"))
            {
                sortText = sortText.Replace("BasePrice", "BasePrice.Amount");
            }

            IQueryable<Product> queryable = _context.Set<Product>().AsNoTracking();

            var total = await queryable.CountAsync(cancellationToken);

            if (!string.IsNullOrWhiteSpace(query.SearchText))
            {
                var search = query.SearchText.Trim();
                queryable = queryable.Where(x => x.Name.Contains(search) || x.Sku.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(query.StockFilter))
            {
                var filter = query.StockFilter.Trim().ToLowerInvariant();
                if (filter == "in_stock")
                {
                    queryable = queryable.Where(x => x.Variants.Any(v => v.StockQuantity > 0));
                }
                else if (filter == "low_stock")
                {
                    queryable = queryable.Where(x => x.Variants.Any(v => v.StockQuantity <= v.lowStockQuantity && v.StockQuantity > 0));
                }
                else if (filter == "out_of_stock")
                {
                    queryable = queryable.Where(x => !x.Variants.Any() || x.Variants.All(v => v.StockQuantity == 0));
                }
                else if (filter == "on_sale")
                {
                    var now = DateTime.UtcNow;
                    var activeDealProductIds = _context.Set<DealCountdown>()
                        .Where(d => d.IsActive && (!d.StartDate.HasValue || d.StartDate <= now) && d.EndDate >= now)
                        .Select(d => d.ProductId);

                    queryable = queryable.Where(x => (x.SalePrice != null && x.SalePrice.Amount < x.BasePrice.Amount) || activeDealProductIds.Contains(x.Id));
                }
            }

            var totalDisplay = await queryable.CountAsync(cancellationToken);

            queryable = queryable
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Include(p => p.Images)
                .Include(p => p.Variants);

            try
            {
                queryable = queryable.OrderBy(sortText);
            }
            catch
            {
                queryable = queryable.OrderBy(p => p.Name);
            }

            var items = await queryable
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return (items, total, totalDisplay);
        }

        public async Task<Product?> GetProductDetailsById(Guid id, CancellationToken cancellationToken)
        {
            var list = await GetAsync(
                x => x.Id == id,
                x => x.Include(p => p.Category)
                      .Include(p => p.Brand)
                      .Include(p => p.Images)
                      .Include(p => p.Variants)
                      .Include(p => p.Tags),
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

        public async Task<ProductCatalogStatsDto> GetCatalogStatsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var totalProducts = await _context.Set<Product>().CountAsync(cancellationToken);
            var totalStockUnits = await _context.Set<ProductVariant>().SumAsync(v => (int?)v.StockQuantity, cancellationToken) ?? 0;
            var lowStockAlertCount = await _context.Set<Product>().CountAsync(
                p => p.Variants.Any(v => v.StockQuantity <= v.lowStockQuantity && v.StockQuantity > 0),
                cancellationToken);
            var outOfStockCount = await _context.Set<Product>().CountAsync(
                p => !p.Variants.Any() || p.Variants.All(v => v.StockQuantity == 0),
                cancellationToken);
            var activeDealsCount = await _context.Set<DealCountdown>().CountAsync(
                d => d.IsActive && (!d.StartDate.HasValue || d.StartDate <= now) && d.EndDate >= now,
                cancellationToken);

            return new ProductCatalogStatsDto
            {
                TotalProducts = totalProducts,
                TotalStockUnits = totalStockUnits,
                LowStockAlertCount = lowStockAlertCount,
                OutOfStockCount = outOfStockCount,
                ActiveDealsCount = activeDealsCount
            };
        }

        public async Task<IList<ProductVariantStockDto>> GetVariantsByProductIdAsync(Guid productId, CancellationToken cancellationToken)
        {
            var variants = await _context.Set<ProductVariant>()
                .AsNoTracking()
                .Where(v => v.ProductId == productId)
                .OrderBy(v => v.Size)
                .ThenBy(v => v.ColorName)
                .ToListAsync(cancellationToken);

            return variants.Select(v =>
            {
                string healthStatus = "Healthy";
                if (v.StockQuantity == 0)
                    healthStatus = "OutOfStock";
                else if (v.StockQuantity <= v.lowStockQuantity)
                    healthStatus = "LowStock";

                return new ProductVariantStockDto
                {
                    Id = v.Id,
                    ProductId = v.ProductId,
                    Size = v.Size,
                    ColorName = v.ColorName,
                    ColorHex = v.ColorHex,
                    Sku = v.Sku,
                    StockQuantity = v.StockQuantity,
                    LowStockQuantity = v.lowStockQuantity,
                    VariantPrice = v.VariantPrice?.Amount,
                    HealthStatus = healthStatus
                };
            }).ToList();
        }

        public async Task<bool> UpdateVariantStockAsync(Guid variantId, int newQuantity, CancellationToken cancellationToken)
        {
            var variant = await _context.Set<ProductVariant>().FirstOrDefaultAsync(v => v.Id == variantId, cancellationToken);
            if (variant == null)
                return false;

            variant.StockQuantity = Math.Max(0, newQuantity);
            variant.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Dictionary<Guid, ProductActiveDealDto>> GetActiveDealsForProductIdsAsync(List<Guid> productIds, CancellationToken cancellationToken)
        {
            if (productIds == null || productIds.Count == 0)
                return new Dictionary<Guid, ProductActiveDealDto>();

            var now = DateTime.UtcNow;
            var deals = await _context.Set<DealCountdown>()
                .AsNoTracking()
                .Include(d => d.Product)
                .Where(d => productIds.Contains(d.ProductId) && d.IsActive && (!d.StartDate.HasValue || d.StartDate <= now) && d.EndDate >= now)
                .ToListAsync(cancellationToken);

            var result = new Dictionary<Guid, ProductActiveDealDto>();
            foreach (var deal in deals)
            {
                if (!result.ContainsKey(deal.ProductId))
                {
                    decimal discountPercent = 0;
                    if (deal.Product != null && deal.Product.BasePrice != null && deal.Product.BasePrice.Amount > 0)
                    {
                        var original = deal.Product.BasePrice.Amount;
                        var sale = deal.SalePrice.Amount;
                        if (original > sale)
                        {
                            discountPercent = Math.Round(((original - sale) / original) * 100, 0);
                        }
                    }

                    result[deal.ProductId] = new ProductActiveDealDto
                    {
                        DealId = deal.Id,
                        ProductId = deal.ProductId,
                        DealTitle = deal.DealTitle,
                        SalePrice = deal.SalePrice.Amount,
                        DiscountPercent = discountPercent,
                        StartDate = deal.StartDate,
                        EndDate = deal.EndDate,
                        IsLive = true
                    };
                }
            }

            return result;
        }

        public async Task<List<Coupon>> GetActiveStoreCouponsAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            return await _context.Set<Coupon>()
                .AsNoTracking()
                .Where(c => c.IsActive && (!c.StartDate.HasValue || c.StartDate <= now) && c.ExpirationDate >= now)
                .OrderBy(c => c.ExpirationDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Product>> GetFeaturedHomeProductsAsync(int take, CancellationToken cancellationToken)
        {
            return await _context.Set<Product>()
                .AsNoTracking()
                .Where(p => p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.IsNewArrival || p.IsHotSale)
                .ThenByDescending(p => p.Rating)
                .ThenByDescending(p => p.CreatedAt)
                .Take(take > 0 ? take : 8)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Product>> GetRelatedProductsAsync(Guid categoryId, Guid excludeProductId, int take, CancellationToken cancellationToken)
        {
            return await _context.Set<Product>()
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId && p.Id != excludeProductId && p.IsActive)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .OrderByDescending(p => p.CreatedAt)
                .Take(take > 0 ? take : 4)
                .ToListAsync(cancellationToken);
        }

        public async Task<IList<Product>> GetProductsByIdsAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            if (ids == null || !ids.Any())
                return new List<Product>();

            return await _context.Set<Product>()
                .AsNoTracking()
                .Where(p => ids.Contains(p.Id) && p.IsActive)
                .Include(p => p.Images)
                .Include(p => p.Variants)
                .ToListAsync(cancellationToken);
        }
    }
}

