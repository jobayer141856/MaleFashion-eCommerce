using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Products.Commands;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Entities;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Promotions;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Catalogs;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Catalogs
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,CatalogManager")]
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public ProductsController(
            ILogger<ProductsController> logger,
            IMediator mediator,
            IMapper mapper,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var stats = await _mediator.SendQueryAsync<GetProductCatalogStatsQuery, ProductCatalogStatsDto>(
                    new GetProductCatalogStatsQuery(), cancellationToken);
                ViewBag.CatalogStats = stats;

                var activeCoupons = await _mediator.SendQueryAsync<GetActiveStoreCouponsQuery, List<Coupon>>(
                    new GetActiveStoreCouponsQuery(), cancellationToken);
                ViewBag.ActiveCoupons = activeCoupons;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load product catalog metrics.");
                ViewBag.CatalogStats = new ProductCatalogStatsDto();
                ViewBag.ActiveCoupons = new List<Coupon>();
            }

            return View();
        }

        public IActionResult Create()
        {
            var model = new ProductModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? primaryImageName = null;
                    if (model.PrimaryImage != null)
                    {
                        await using var stream = model.PrimaryImage.OpenReadStream();
                        primaryImageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.PrimaryImage.FileName,
                            "products",
                            cancellationToken);
                    }

                    var galleryImageNames = new List<string>();
                    if (model.GalleryImages != null && model.GalleryImages.Count > 0)
                    {
                        foreach (var file in model.GalleryImages)
                        {
                            await using var gStream = file.OpenReadStream();
                            var gName = await _fileStorageService.SaveImageAsync(
                                gStream,
                                file.FileName,
                                "products",
                                cancellationToken);
                            galleryImageNames.Add(gName);
                        }
                    }

                    var command = new ProductAddCommand
                    {
                        Name = model.Name,
                        Sku = model.Sku,
                        Description = model.Description,
                        AdditionalInformation = model.AdditionalInformation,
                        BasePrice = model.BasePrice,
                        SalePrice = model.SalePrice,
                        CategoryId = model.CategoryId,
                        BrandId = model.BrandId,
                        IsActive = model.IsActive,
                        IsNewArrival = model.IsNewArrival,
                        IsHotSale = model.IsHotSale,
                        PrimaryImageName = primaryImageName,
                        GalleryImageNames = galleryImageNames,
                        Variants = model.Variants.Select(v => new ProductVariantDto
                        {
                            Size = v.Size,
                            ColorName = v.ColorName,
                            ColorHex = v.ColorHex,
                            Sku = v.Sku,
                            StockQuantity = v.StockQuantity,
                            LowStockQuantity = v.LowStockQuantity,
                            Price = v.Price
                        }).ToList()
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Product successfully created.",
                        Type = ResponseTypes.Success
                    });

                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException oex)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = oex.Message,
                        Type = ResponseTypes.Danger
                    });
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to create product.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = errorMessage,
                        Type = ResponseTypes.Danger
                    });
                }
            }
            else
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Please provide all required product information.",
                    Type = ResponseTypes.Danger
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Update(Guid id)
        {
            try
            {
                var query = new GetProductByIdQuery { Id = id };
                var product = await _mediator.SendQueryAsync(query);

                if (product == null)
                {
                    throw new Exception("Product not found.");
                }

                var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary);
                var galleryImages = product.Images.Where(i => !i.IsPrimary).ToList();

                var model = new ProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    Sku = product.Sku,
                    Description = product.Description,
                    AdditionalInformation = product.AdditionalInformation,
                    BasePrice = product.BasePrice.Amount,
                    SalePrice = product.SalePrice?.Amount,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name,
                    BrandId = product.BrandId,
                    BrandName = product.Brand?.Name,
                    IsActive = product.IsActive,
                    IsNewArrival = product.IsNewArrival,
                    IsHotSale = product.IsHotSale,
                    PrimaryImageUrl = primaryImage?.ImageUrl,
                    ExistingGalleryImages = galleryImages.Select(g => new ProductImageItemModel
                    {
                        Id = g.Id,
                        ImageUrl = g.ImageUrl,
                        IsPrimary = g.IsPrimary
                    }).ToList(),
                    Variants = product.Variants.Select(v => new ProductVariantItemModel
                    {
                        Id = v.Id,
                        Size = v.Size,
                        ColorName = v.ColorName,
                        ColorHex = v.ColorHex,
                        Sku = v.Sku,
                        StockQuantity = v.StockQuantity,
                        LowStockQuantity = v.lowStockQuantity,
                        Price = v.VariantPrice?.Amount
                    }).ToList()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to load product.";
                _logger.LogError(ex, errorMessage);
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = errorMessage,
                    Type = ResponseTypes.Danger
                });

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(ProductModel model, List<Guid>? deletedImageIds, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var query = new GetProductByIdQuery { Id = model.Id };
                    var product = await _mediator.SendQueryAsync(query, cancellationToken);

                    if (product == null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = "Product doesn't exist.",
                            Type = ResponseTypes.Danger
                        });
                        return RedirectToAction(nameof(Index));
                    }

                    var existingPrimary = product.Images.FirstOrDefault(i => i.IsPrimary);
                    var oldPrimaryName = existingPrimary?.ImageUrl;
                    var primaryImageName = oldPrimaryName;

                    if (model.PrimaryImage != null)
                    {
                        await using var stream = model.PrimaryImage.OpenReadStream();
                        primaryImageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.PrimaryImage.FileName,
                            "products",
                            cancellationToken);
                    }

                    var newGalleryImageNames = new List<string>();
                    if (model.GalleryImages != null && model.GalleryImages.Count > 0)
                    {
                        foreach (var file in model.GalleryImages)
                        {
                            await using var gStream = file.OpenReadStream();
                            var gName = await _fileStorageService.SaveImageAsync(
                                gStream,
                                file.FileName,
                                "products",
                                cancellationToken);
                            newGalleryImageNames.Add(gName);
                        }
                    }

                    var command = new ProductUpdateCommand
                    {
                        Id = model.Id,
                        Name = model.Name,
                        Sku = model.Sku,
                        Description = model.Description,
                        AdditionalInformation = model.AdditionalInformation,
                        BasePrice = model.BasePrice,
                        SalePrice = model.SalePrice,
                        CategoryId = model.CategoryId,
                        BrandId = model.BrandId,
                        IsActive = model.IsActive,
                        IsNewArrival = model.IsNewArrival,
                        IsHotSale = model.IsHotSale,
                        PrimaryImageName = primaryImageName,
                        NewGalleryImageNames = newGalleryImageNames,
                        DeletedImageIds = deletedImageIds ?? new List<Guid>(),
                        Variants = model.Variants.Select(v => new ProductVariantDto
                        {
                            Id = v.Id,
                            Size = v.Size,
                            ColorName = v.ColorName,
                            ColorHex = v.ColorHex,
                            Sku = v.Sku,
                            StockQuantity = v.StockQuantity,
                            LowStockQuantity = v.LowStockQuantity,
                            Price = v.Price
                        }).ToList()
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    // Delete old primary image if replaced
                    if (model.PrimaryImage != null && !string.IsNullOrEmpty(oldPrimaryName) && oldPrimaryName != primaryImageName)
                    {
                        try
                        {
                            await _fileStorageService.DeleteImageAsync(oldPrimaryName, "products", cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to delete old primary image for product ID: {model.Id}");
                        }
                    }

                    // Delete removed gallery image files
                    if (deletedImageIds != null && deletedImageIds.Count > 0)
                    {
                        var removedImages = product.Images.Where(i => deletedImageIds.Contains(i.Id)).ToList();
                        foreach (var rImg in removedImages)
                        {
                            try
                            {
                                await _fileStorageService.DeleteImageAsync(rImg.ImageUrl, "products", cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Failed to delete removed gallery image '{rImg.ImageUrl}'");
                            }
                        }
                    }

                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Product successfully updated.",
                        Type = ResponseTypes.Success
                    });

                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException oex)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = oex.Message,
                        Type = ResponseTypes.Danger
                    });
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to update product.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = errorMessage,
                        Type = ResponseTypes.Danger
                    });
                }
            }
            else
            {
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Please provide all required product information.",
                    Type = ResponseTypes.Danger
                });
            }

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedProducts([FromBody] ProductListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = _mapper.Map<GetAllProductsByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("Name", "Name", "Category.Name", "Name", "BasePrice.Amount", "IsActive", "Id");
                query.StockFilter = model.StockFilter;

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllProductsByPagingQuery,
                    (IList<Product>, int, int)>(query, cancellationToken);

                var productIds = items.Select(x => x.Id).ToList();
                var activeDeals = await _mediator.SendQueryAsync<GetActiveDealsForProductsQuery,
                    Dictionary<Guid, ProductActiveDealDto>>(new GetActiveDealsForProductsQuery { ProductIds = productIds }, cancellationToken);

                var now = DateTime.UtcNow;

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let primaryImg = item.Images.FirstOrDefault(i => i.IsPrimary) ?? item.Images.FirstOrDefault()
                            let imgSrc = primaryImg != null && !string.IsNullOrEmpty(primaryImg.ImageUrl)
                                ? $"/uploads/products/{HttpUtility.HtmlEncode(primaryImg.ImageUrl)}"
                                : "/sneat/assets/img/elements/1.jpg"
                            let nameAndSkuHtml = $@"<div>
                                <a href='/Admin/Products/Update/{item.Id}' class='fw-semibold text-heading d-block'>{HttpUtility.HtmlEncode(item.Name)}</a>
                                <small class='text-muted'><i class='bx bx-barcode me-1'></i>SKU: <code>{HttpUtility.HtmlEncode(item.Sku)}</code></small>
                            </div>"
                            let catAndBrandHtml = $@"<div>
                                <span class='badge bg-label-primary mb-1'>{HttpUtility.HtmlEncode(item.Category?.Name ?? "Uncategorized")}</span>
                                <div class='small text-muted'><i class='bx bx-purchase-tag-alt me-1'></i>{HttpUtility.HtmlEncode(item.Brand?.Name ?? "No Brand")}</div>
                            </div>"
                            let totalStock = item.Variants != null && item.Variants.Count > 0 ? item.Variants.Sum(v => v.StockQuantity) : 0
                            let lowStockCount = item.Variants != null ? item.Variants.Count(v => v.StockQuantity <= v.lowStockQuantity && v.StockQuantity > 0) : 0
                            let variantCount = item.Variants?.Count ?? 0
                            let stockBadge = variantCount == 0
                                ? "<span class='badge bg-label-secondary'>No Variants</span>"
                                : totalStock == 0
                                    ? "<span class='badge bg-label-danger'><i class='bx bx-x-circle me-1'></i>0 units (Out of Stock)</span>"
                                    : lowStockCount > 0
                                        ? $"<span class='badge bg-label-warning text-dark'><i class='bx bx-error-circle me-1'></i>{totalStock} units</span> <span class='badge bg-danger text-white ms-1' title='Low stock'>{lowStockCount} Low</span>"
                                        : $"<span class='badge bg-label-success'><i class='bx bx-check-circle me-1'></i>{totalStock} units</span>"
                            let stockHtml = $@"<div>
                                <div>{stockBadge}</div>
                                <button type='button' class='btn btn-xs btn-outline-primary mt-1 view-variants-btn' data-id='{item.Id}' data-name='{HttpUtility.HtmlEncode(item.Name)}'>
                                    <i class='bx bx-layer me-1'></i>{variantCount} Variants
                                </button>
                            </div>"
                            let deal = activeDeals != null && activeDeals.ContainsKey(item.Id) ? activeDeals[item.Id] : null
                            let hasSalePrice = item.SalePrice != null && item.SalePrice.Amount < item.BasePrice.Amount
                            let priceHtml = deal != null
                                ? $@"<div>
                                    <del class='text-muted small'>৳{item.BasePrice.Amount:N2}</del>
                                    <span class='fw-bold text-danger ms-1'>৳{deal.SalePrice:N2}</span>
                                    <span class='badge bg-label-danger ms-1'>-{deal.DiscountPercent:F0}%</span>
                                    <div class='mt-1'>
                                        <span class='badge bg-danger text-white' title='Flash Deal: {HttpUtility.HtmlEncode(deal.DealTitle)}'>
                                            <i class='bx bxs-zap me-1'></i>{HttpUtility.HtmlEncode(deal.DealTitle)}
                                        </span>
                                    </div>
                                    <small class='text-danger d-block mt-1' style='font-size: 0.75rem;'>
                                        <i class='bx bx-time-five me-1'></i>Ends {(int)Math.Max(1, Math.Ceiling((deal.EndDate - now).TotalDays))}d ({deal.EndDate:MMM dd})
                                    </small>
                                </div>"
                                : hasSalePrice
                                    ? $@"<div>
                                        <del class='text-muted small'>৳{item.BasePrice.Amount:N2}</del>
                                        <span class='fw-bold text-success ms-1'>৳{item.SalePrice!.Amount:N2}</span>
                                        <span class='badge bg-label-success ms-1'>-{Math.Round(((item.BasePrice.Amount - item.SalePrice.Amount) / item.BasePrice.Amount) * 100, 0):F0}%</span>
                                        <div class='mt-1'>
                                            <span class='badge bg-label-info'><i class='bx bx-tag me-1'></i>Regular Sale</span>
                                        </div>
                                    </div>"
                                    : $@"<div>
                                        <span class='fw-semibold text-dark'>৳{item.BasePrice.Amount:N2}</span>
                                    </div>"
                            select new string[]
                            {
                                imgSrc,
                                nameAndSkuHtml,
                                catAndBrandHtml,
                                stockHtml,
                                priceHtml,
                                item.IsActive ? "Active" : "Inactive",
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get product list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetVariants(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var variants = await _mediator.SendQueryAsync<GetProductVariantsWithStockQuery, IList<ProductVariantStockDto>>(
                    new GetProductVariantsWithStockQuery { ProductId = id }, cancellationToken);

                return Json(new { success = true, data = variants });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch variants for product {ProductId}", id);
                return Json(new { success = false, message = "Failed to load variants." });
            }
        }

        public class QuickStockUpdateRequest
        {
            public Guid VariantId { get; set; }
            public int NewStockQuantity { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> QuickUpdateStock([FromBody] QuickStockUpdateRequest request, CancellationToken cancellationToken)
        {
            if (request.NewStockQuantity < 0)
            {
                return Json(new { success = false, message = "Stock quantity cannot be negative." });
            }

            try
            {
                var command = new ProductVariantQuickUpdateStockCommand
                {
                    VariantId = request.VariantId,
                    NewStockQuantity = request.NewStockQuantity
                };

                var result = await _mediator.SendCommandAsync(command, cancellationToken);
                if (result)
                {
                    return Json(new { success = true, message = $"Stock successfully updated to {request.NewStockQuantity} units." });
                }
                else
                {
                    return Json(new { success = false, message = "Variant not found." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to quick update stock for variant {VariantId}", request.VariantId);
                return Json(new { success = false, message = "Failed to update stock." });
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleteCommand = new ProductDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(deleteCommand);

                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = "Product successfully deleted.",
                    Type = ResponseTypes.Success
                });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to delete product.";
                _logger.LogError(ex, errorMessage);
                TempData.Put(Constants.ResponseTempKey, new ResponseModel
                {
                    Message = errorMessage,
                    Type = ResponseTypes.Danger
                });
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult IndexSP()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedProductsSP([FromBody] ProductListSPModel model, CancellationToken cancellationToken)
        {
            var query = new GetShopProductsQuery
            {
                PageIndex = (model.Start / (model.Length > 0 ? model.Length : 10)) + 1,
                PageSize = model.Length > 0 ? model.Length : 10,
                SearchText = model.Name,
                PriceFrom = model.PriceFrom.HasValue ? Convert.ToDecimal(model.PriceFrom.Value) : null,
                PriceTo = model.PriceTo.HasValue ? Convert.ToDecimal(model.PriceTo.Value) : null
            };

            var (products, total, totalDisplay) = await _mediator.SendQueryAsync<GetShopProductsQuery,
                (IList<ShopProductItemDto>, int, int)>(query, cancellationToken);

            var data = products.Select(p => new object[]
            {
                !string.IsNullOrEmpty(p.ImageUrl) ? (p.ImageUrl.StartsWith("/") ? p.ImageUrl : $"/uploads/products/{p.ImageUrl}") : "/sneat/assets/img/elements/1.jpg",
                p.Name,
                $"৳{p.EffectivePrice:N2}",
                p.Id
            });

            return Json(new
            {
                recordsTotal = total,
                recordsFiltered = totalDisplay,
                data
            });
        }
    }
}

