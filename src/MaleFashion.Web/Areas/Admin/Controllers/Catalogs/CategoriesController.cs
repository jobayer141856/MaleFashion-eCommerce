using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Catalogs.Categories.Commands;
using MaleFashion.Application.Features.Catalogs.Categories.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Catalogs;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Catalogs
{
    [Area("Admin")]
    public class CategoriesController : Controller
    {
        private readonly ILogger<CategoriesController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public CategoriesController(
            ILogger<CategoriesController> logger,
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
            var model = new CategoryModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                   // Console.WriteLine($"Model received: Name={model.Name}, IsActive={model.IsActive}, Image={model.Image?.FileName}");
                    string? imageName = null;
                    if (model.Image != null)
                    {
                        await using var stream = model.Image.OpenReadStream();

                        imageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Image.FileName,
                            "categories",
                            cancellationToken);
                    }

                    var command = _mapper.Map<CategoryAddCommand>(model);
                    command.ImageName = imageName;

                    var result = await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Category successfully created.",
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
                    const string errorMessage = "Failed to create category.";

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
                var query = new GetCategoryByIdQuery { Id = id };
                var result = await _mediator.SendQueryAsync(query);

                if (result != null)
                {
                    var model = _mapper.Map<CategoryModel>(result);
                    // Pre-populate the currently saved Parent Category for the Select2 dropdown
                    if (result.ParentCategoryId.HasValue)
                    {
                        var parentCategory = await _mediator.SendQueryAsync(new GetCategoryByIdQuery { Id = result.ParentCategoryId.Value });
                        if (parentCategory != null)
                        {
                            model.ParentCategories = new List<SelectListItem>
                    {
                        new SelectListItem
                        {
                            Value = parentCategory.Id.ToString(),
                            Text = parentCategory.Name,
                            Selected = true
                        }
                    };
                        }
                    }
                    return View(model);
                }
                else
                {
                    throw new Exception("Category not found.");
                }
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to load category.";

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
        public async Task<IActionResult> Update(CategoryModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var query = new GetCategoryByIdQuery { Id = model.Id };
                    var category = await _mediator.SendQueryAsync(query, cancellationToken);

                    if (category is null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = "Category doesn't exist.",
                            Type = ResponseTypes.Danger
                        });

                        return RedirectToAction(nameof(Index));
                    }

                    var oldImageName = category.ImageName;
                    var imageName = oldImageName;

                    if (model.Image != null)
                    {
                        await using var stream = model.Image.OpenReadStream();

                        imageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Image.FileName,
                            "categories",
                            cancellationToken);
                    }

                    var command = _mapper.Map<CategoryUpdateCommand>(model);
                    command.ImageName = imageName;
                    var result = await _mediator.SendCommandAsync(command, cancellationToken);

                    // Delete old image if a new image was uploaded
                    if (model.Image != null && !string.IsNullOrEmpty(oldImageName) && oldImageName != imageName)
                    {
                        try
                        {
                            await _fileStorageService.DeleteImageAsync(
                                oldImageName,
                                "categories",
                                cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, $"Failed to delete old image for category ID: {model.Id}");
                        }
                    }

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Category successfully updated.",
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
                    const string errorMessage = "Failed to update category.";

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
        public async Task<JsonResult> GetPagedCategories([FromBody] CategoryListModel model)
        {
            try
            {
                var query = _mapper.Map<GetAllCategoriesByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("ImageName", "Name", "IsActive", "Id");

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllCategoriesByPagingQuery,
                    (IList<Category>, int, int)>(query);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                !string.IsNullOrEmpty(item.ImageName)
                                    ? $"/uploads/categories/{HttpUtility.HtmlEncode(item.ImageName)}"
                                    : "/sneat/assets/img/elements/1.jpg",
                                HttpUtility.HtmlEncode(item.Name),
                                item.ParentCategory != null ? HttpUtility.HtmlEncode(item.ParentCategory.Name) : "N/A",
                                item.IsActive ? "Active" : "Inactive",
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get category list");

                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleteCommand = new CategoryDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(deleteCommand);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Category successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to delete category.";

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
        public async Task<IActionResult> GetAllCategories(string? q, string? searchText)
        {
            try
            {
                var term = !string.IsNullOrWhiteSpace(q) ? q : searchText;

                var query = new GetAllCategoriesQuery
                {
                    ActiveOnly = true,
                    SearchText = term
                };

                var result = await _mediator.SendQueryAsync(query);

                var data = result.Select(c => new
                {
                    id = c.Id,
                    name = c.Name
                });

                return Json(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get all categories.");
                return StatusCode(500, new { message = "Failed to load categories." });
            }
        }
    }
}