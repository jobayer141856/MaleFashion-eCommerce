using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Brands.Commands;
using MaleFashion.Application.Features.Catalogs.Brands.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Catalogs;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Catalogs
{
        [Area("Admin")]
        [Authorize(Roles = "Admin,SuperAdmin,CatalogManager")]
        public class BrandsController : Controller
        {
            private readonly ILogger<BrandsController> _logger;
            private readonly IMediator _mediator;
            private readonly IMapper _mapper;
            private readonly IFileStorageService _fileStorageService;
            public BrandsController(
                ILogger<BrandsController> logger,
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
                var model = new BrandModel();
                return View(model);
            }
            [HttpPost, ValidateAntiForgeryToken]
            public async Task<IActionResult> Create(BrandModel model, CancellationToken cancellationToken)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        string? logoUrl = null;
                        if (model.Logo != null)
                        {
                            await using var stream = model.Logo.OpenReadStream();
                            logoUrl = await _fileStorageService.SaveImageAsync(
                                stream,
                                model.Logo.FileName,
                                "brands",
                                cancellationToken);
                        }
                        var command = _mapper.Map<BrandAddCommand>(model);
                        command.LogoUrl = logoUrl;
                        var result = await _mediator.SendCommandAsync(command, cancellationToken);
                        TempData.Put(Constants.ResponseTempKey,
                            new ResponseModel
                            {
                                Message = "Brand successfully created.",
                                Type = ResponseTypes.Success
                            });
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DuplicateDataException oex)
                    {
                        TempData.Put(Constants.ResponseTempKey,
                            new ResponseModel
                            {
                                Message = oex.Message,
                                Type = ResponseTypes.Danger
                            });
                    }
                    catch (Exception ex)
                    {
                        const string errorMessage = "Failed to create brand.";
                        _logger.LogError(ex, errorMessage);
                        TempData.Put(Constants.ResponseTempKey,
                            new ResponseModel
                            {
                                Message = errorMessage,
                                Type = ResponseTypes.Danger
                            });
                    }
                }
                else
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Please provide all required information.",
                            Type = ResponseTypes.Danger
                        });
                }
                return View(model);
            }
            public async Task<IActionResult> Update(Guid id)
            {
                try
                {
                    var query = new GetBrandByIdQuery { Id = id };
                    var result = await _mediator.SendQueryAsync(query);
                    if (result != null)
                    {
                        var model = _mapper.Map<BrandModel>(result);
                        return View(model);
                    }
                    else
                    {
                        throw new Exception("Brand not found.");
                    }
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to load brand.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = errorMessage,
                            Type = ResponseTypes.Danger
                        });
                    return RedirectToAction(nameof(Index));
                }
            }
            [HttpPost, ValidateAntiForgeryToken]
            public async Task<IActionResult> Update(BrandModel model, CancellationToken cancellationToken)
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        var query = new GetBrandByIdQuery { Id = model.Id };
                        var brand = await _mediator.SendQueryAsync(query, cancellationToken);
                        if (brand is null)
                        {
                            TempData.Put(Constants.ResponseTempKey, new ResponseModel
                            {
                                Message = "Brand doesn't exist.",
                                Type = ResponseTypes.Danger
                            });
                            return RedirectToAction(nameof(Index));
                        }
                        var oldLogoUrl = brand.LogoUrl;
                        var logoUrl = oldLogoUrl;
                        if (model.Logo != null)
                        {
                            await using var stream = model.Logo.OpenReadStream();
                            logoUrl = await _fileStorageService.SaveImageAsync(
                                stream,
                                model.Logo.FileName,
                                "brands",
                                cancellationToken);
                        }
                        var command = _mapper.Map<BrandUpdateCommand>(model);
                        command.LogoUrl = logoUrl;
                        var result = await _mediator.SendCommandAsync(command, cancellationToken);
                        // Delete old logo if a replacement was uploaded
                        if (model.Logo != null && !string.IsNullOrEmpty(oldLogoUrl) && oldLogoUrl != logoUrl)
                        {
                            try
                            {
                                await _fileStorageService.DeleteImageAsync(oldLogoUrl, "brands", cancellationToken);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, $"Failed to delete old logo for brand ID: {model.Id}");
                            }
                        }
                        TempData.Put(Constants.ResponseTempKey,
                            new ResponseModel
                            {
                                Message = "Brand successfully updated.",
                                Type = ResponseTypes.Success
                            });
                        return RedirectToAction(nameof(Index));
                    }
                    catch (DuplicateDataException oex)
                    {
                        TempData.Put(Constants.ResponseTempKey,
                            new ResponseModel
                            {
                                Message = oex.Message,
                                Type = ResponseTypes.Danger
                            });
                    }
                    catch (Exception ex)
                    {
                        const string errorMessage = "Failed to update brand.";
                        _logger.LogError(ex, errorMessage);
                        TempData.Put(Constants.ResponseTempKey,
                            new ResponseModel
                            {
                                Message = errorMessage,
                                Type = ResponseTypes.Danger
                            });
                    }
                }
                else
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Please provide all required information.",
                            Type = ResponseTypes.Danger
                        });
                }
                return View(model);
            }
            [HttpPost]
            public async Task<JsonResult> GetPagedBrands([FromBody] BrandListModel model)
            {
                try
                {
                    var query = _mapper.Map<GetAllBrandsByPagingQuery>(model);
                    query.SearchText = model.Search.Value;
                    query.SortText = model.FormatSortExpression("LogoUrl", "Name", "IsActive", "Id");
                    var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllBrandsByPagingQuery,
                        (IList<Brand>, int, int)>(query);
                    var response = new
                    {
                        recordsTotal = total,
                        recordsFiltered = totalDisplay,
                        data = (from item in items
                                select new string[]
                                {
                                !string.IsNullOrEmpty(item.LogoUrl)
                                    ? $"/uploads/brands/{HttpUtility.HtmlEncode(item.LogoUrl)}"
                                    : "/sneat/assets/img/elements/1.jpg",
                                HttpUtility.HtmlEncode(item.Name),
                                item.IsActive ? "Active" : "Inactive",
                                item.Id.ToString()
                                }).ToArray()
                    };
                    return Json(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get brand list");
                    return Json(DataTables.EmptyResult);
                }
            }
            [HttpPost, ValidateAntiForgeryToken]
            public async Task<IActionResult> Delete(Guid id)
            {
                try
                {
                    var deleteCommand = new BrandDeleteCommand { Id = id };
                    await _mediator.SendCommandAsync(deleteCommand);
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Brand successfully deleted.",
                            Type = ResponseTypes.Success
                        });
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to delete brand.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = errorMessage,
                            Type = ResponseTypes.Danger
                        });
                }
                return RedirectToAction(nameof(Index));
            }
            [HttpGet]
            public async Task<IActionResult> GetAllBrands(string? q, string? searchText)
            {
                try
                {
                    var term = !string.IsNullOrWhiteSpace(q) ? q : searchText;
                    var query = new GetAllBrandsQuery
                    {
                        ActiveOnly = true,
                        SearchText = term
                    };
                    var result = await _mediator.SendQueryAsync(query);
                    var data = result.Select(b => new { id = b.Id, name = b.Name });
                    return Json(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to get all brands.");
                    return StatusCode(500, new { message = "Failed to load brands." });
                }
            }
        }
}


