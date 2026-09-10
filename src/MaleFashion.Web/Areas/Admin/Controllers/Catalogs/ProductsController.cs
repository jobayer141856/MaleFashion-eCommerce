using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Products.Commands;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Entities;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Catalogs;
using MapsterMapper;
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

        public IActionResult Index()
        {
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
        public async Task<JsonResult> GetPagedProducts([FromBody] ProductListModel model)
        {
            try
            {
                var query = _mapper.Map<GetAllProductsByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("Name", "Name", "Sku", "Category.Name", "Brand.Name", "BasePrice.Amount", "IsActive", "Id");

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllProductsByPagingQuery,
                    (IList<Product>, int, int)>(query);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let primaryImg = item.Images.FirstOrDefault(i => i.IsPrimary) ?? item.Images.FirstOrDefault()
                            select new string[]
                            {
                                primaryImg != null && !string.IsNullOrEmpty(primaryImg.ImageUrl)
                                    ? $"/uploads/products/{HttpUtility.HtmlEncode(primaryImg.ImageUrl)}"
                                    : "/sneat/assets/img/elements/1.jpg",
                                HttpUtility.HtmlEncode(item.Name),
                                HttpUtility.HtmlEncode(item.Sku),
                                HttpUtility.HtmlEncode(item.Category?.Name ?? "N/A"),
                                HttpUtility.HtmlEncode(item.Brand?.Name ?? "N/A"),
                                $"৳{item.BasePrice.Amount:N2}",
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
    }
}

