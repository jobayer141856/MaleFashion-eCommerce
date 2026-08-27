using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Domain.Entities;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public ProductsController(ILogger<ProductsController> logger,
            IMediator mediator, IMapper mapper, IFileStorageService fileStorageService)
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

        //public IActionResult IndexSP()
        //{
        //   // var model = new ProductListSPModel();
        //   // return View(model);
        //}

        //public IActionResult Create()
        //{
        //    var model = new ProductModel();
        //    return View(model);
        //}

        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(ProductModel model, CancellationToken cancellationToken)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            string? imageName = null;
        //            if(model.Image != null) 
        //            {
        //                await using var stream = model.Image.OpenReadStream();

        //                imageName = await _fileStorageService.SaveImageAsync(
        //                    stream,
        //                    model.Image.FileName,
        //                    "images",
        //                    cancellationToken);
        //            }

        //            var command = _mapper.Map<ProductAddCommand>(model);
        //            command.ImageName = imageName!;

        //            var result = await _mediator.SendCommandAsync(command);

        //            TempData.Put(Constants.ResponseTempKey,
        //                new ResponseModel
        //                {
        //                    Message = "Product successfully created.",
        //                    Type = ResponseTypes.Success
        //                });

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (DuplicateDataException oex)
        //        {
        //            TempData.Put(Constants.ResponseTempKey,
        //                new ResponseModel
        //                {
        //                    Message = oex.Message,
        //                    Type = ResponseTypes.Danger
        //                });
        //        }
        //        catch (Exception ex)
        //        {
        //            const string errorMessage = "Failed to create product.";

        //            _logger.LogError(ex, errorMessage);

        //            TempData.Put(Constants.ResponseTempKey,
        //                new ResponseModel
        //                {
        //                    Message = errorMessage,
        //                    Type = ResponseTypes.Danger
        //                });
        //        }
        //    }
        //    else
        //    {
        //        TempData.Put(Constants.ResponseTempKey,
        //            new ResponseModel
        //            {
        //                Message = "Please provide all information.",
        //                Type = ResponseTypes.Success
        //            });
        //    }

        //    return View(model);
        //}

        //public async Task<IActionResult> Update(Guid id)
        //{
        //    try
        //    {
        //        var query = new GetProductByIdQuery { Id = id };
        //        var result = await _mediator.SendQueryAsync(query);

        //        if (result != null)
        //        {
        //            var model = _mapper.Map<ProductModel>(result);
        //            return View(model);
        //        }
        //        else
        //            throw new Exception("Failed to load product");
        //    }
        //    catch (Exception ex)
        //    {
        //        const string errorMessage = "Failed to load product.";

        //        _logger.LogError(ex, errorMessage);

        //        TempData.Put(Constants.ResponseTempKey,
        //            new ResponseModel
        //            {
        //                Message = errorMessage,
        //                Type = ResponseTypes.Danger
        //            });

        //        return RedirectToAction(nameof(Index));
        //    }
        //}

        //[HttpPost, ValidateAntiForgeryToken]
        //public async Task<IActionResult> Update(ProductModel model, CancellationToken cancellationToken)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            var query = new GetProductByIdQuery { Id = model.Id };
        //            var product = await _mediator.SendQueryAsync(query, cancellationToken);

        //            if (product is null)
        //            {
        //                TempData.Put(Constants.ResponseTempKey, new ResponseModel
        //                {
        //                    Message = "Product doesn't exist",
        //                    Type = ResponseTypes.Danger
        //                });

        //                return RedirectToAction(nameof(Index));
        //            }

        //            var oldImageName = product.ImageName;
        //            var imageName = oldImageName;

        //            if(model.Image != null)
        //            {
        //                await using var stream = model.Image.OpenReadStream();

        //                imageName = await _fileStorageService.SaveImageAsync(
        //                    stream,
        //                    model.Image.FileName,
        //                    "images",
        //                    cancellationToken);
        //            }

        //            var command = _mapper.Map<ProductUpdateCommand>(model);
        //            command.ImageName = imageName;
        //            var result = await _mediator.SendCommandAsync(command, cancellationToken);

        //            if(model.Image!= null && !string.IsNullOrEmpty(oldImageName) && oldImageName!= imageName)
        //            {
        //                try
        //                {
        //                    await _fileStorageService.DeleteImageAsync(
        //                        oldImageName,
        //                        "images",
        //                        cancellationToken);
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.LogWarning(ex, $"Failed to delete old image for product id: {model.Id}");
        //                }
        //            }

        //            TempData.Put(Constants.ResponseTempKey,
        //                new ResponseModel
        //                {
        //                    Message = "Product successfully updated.",
        //                    Type = ResponseTypes.Success
        //                });

        //            return RedirectToAction(nameof(Index));
        //        }
        //        catch (DuplicateDataException oex)
        //        {
        //            TempData.Put(Constants.ResponseTempKey,
        //                new ResponseModel
        //                {
        //                    Message = oex.Message,
        //                    Type = ResponseTypes.Danger
        //                });
        //        }
        //        catch (Exception ex)
        //        {
        //            const string errorMessage = "Failed to update product.";

        //            _logger.LogError(ex, errorMessage);

        //            TempData.Put(Constants.ResponseTempKey,
        //                new ResponseModel
        //                {
        //                    Message = errorMessage,
        //                    Type = ResponseTypes.Danger
        //                });
        //        }
        //    }
        //    else
        //    {
        //        TempData.Put(Constants.ResponseTempKey,
        //            new ResponseModel
        //            {
        //                Message = "Please provide all information.",
        //                Type = ResponseTypes.Success
        //            });
        //    }

        //    return View(model);
        //}

        //[HttpPost]
        //public async Task<JsonResult> GetPagedProducts([FromBody] ProductListModel model)
        //{
        //    try
        //    {
        //        var query = _mapper.Map<GetAllProductsByPagingQuery>(model);
        //        query.SearchText = model.Search.Value;
        //        query.SortText = model.FormatSortExpression("Name", "Name", "Price", "Name");

        //        var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllProductsByPagingQuery, 
        //            (IList<Product>, int, int)>(query);

        //        var product = new
        //        {
        //            recordsTotal = total,
        //            recordsFiltered = totalDisplay,
        //            data = (from item in items
        //                    select new string[]
        //                    {
        //                        $"/uploads/images/{HttpUtility.HtmlEncode(item.ImageName)}",
        //                        HttpUtility.HtmlEncode(item.Name),
        //                        item.Price.ToString(),
        //                        item.Id.ToString()
        //                    }).ToArray()
        //        };

        //        return Json(product);
        //    }
        //    catch(Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to get product list");

        //        return Json(DataTables.EmptyResult);
        //    }
        //}

        //[HttpPost]
        //public async Task<JsonResult> GetPagedProductsSP([FromBody] ProductListSPModel model)
        //{
        //    try
        //    {
        //        var query = _mapper.Map<GetAllProductsByPagingSPQuery>(model);
        //        query.SortText = query.FormatSortExpression(["Name", "Name", "Price", "Name"]);

        //        var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllProductsByPagingSPQuery,
        //            (IList<PagedProductListDTO>, int, int)>(query);

        //        var product = new
        //        {
        //            recordsTotal = total,
        //            recordsFiltered = totalDisplay,
        //            data = (from item in items
        //                    select new string[]
        //                    {
        //                        $"/uploads/images/{HttpUtility.HtmlEncode(item.ImageName)}",
        //                        HttpUtility.HtmlEncode(item.Name),
        //                        item.Price.ToString(),
        //                        item.Id.ToString()
        //                    }).ToArray()
        //        };

        //        return Json(product);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to get product list");

        //        return Json(DataTables.EmptyResult);
        //    }
        //}

        //[HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "SuperAdmin")]
        //public async Task<IActionResult> Delete(Guid id)
        //{
        //    try
        //    {
        //        var deleteCommand = new ProductDeleteCommand { Id = id };
        //        await _mediator.SendCommandAsync(deleteCommand);

        //        TempData.Put(Constants.ResponseTempKey,
        //            new ResponseModel
        //            {
        //                Message = "Product successfully deleted.",
        //                Type = ResponseTypes.Success
        //            });
        //    }
        //    catch(Exception ex)
        //    {
        //        const string errorMessage = "Failed to delete product.";

        //        _logger.LogError(ex, errorMessage);

        //        TempData.Put(Constants.ResponseTempKey,
        //            new ResponseModel
        //            {
        //                Message = errorMessage,
        //                Type = ResponseTypes.Danger
        //            });
        //    }

        //    return RedirectToAction(nameof(Index));
        //}
    }
}
   
