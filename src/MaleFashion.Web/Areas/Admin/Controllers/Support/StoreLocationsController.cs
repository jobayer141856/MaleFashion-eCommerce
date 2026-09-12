using Cortex.Mediator;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.Support.StoreLocations.Commands;
using MaleFashion.Application.Features.Support.StoreLocations.Queries;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Support;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Support
{
    [Area("Admin")]
    public class StoreLocationsController : Controller
    {
        private readonly ILogger<StoreLocationsController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public StoreLocationsController(
            ILogger<StoreLocationsController> logger,
            IMediator mediator,
            IMapper mapper)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var model = new StoreLocationModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StoreLocationModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = _mapper.Map<StoreLocationAddCommand>(model);
                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Store location successfully created.",
                            Type = ResponseTypes.Success
                        });
                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException ex)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = ex.Message,
                            Type = ResponseTypes.Danger
                        });
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to create store location.";
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
                var query = new GetStoreLocationByIdQuery { Id = id };
                var result = await _mediator.SendQueryAsync(query);
                if (result != null)
                {
                    var model = _mapper.Map<StoreLocationModel>(result);
                    return View(model);
                }
                else
                {
                    throw new Exception("Store location not found.");
                }
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to load store location.";
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
        public async Task<IActionResult> Update(StoreLocationModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var command = _mapper.Map<StoreLocationUpdateCommand>(model);
                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Store location successfully updated.",
                            Type = ResponseTypes.Success
                        });
                    return RedirectToAction(nameof(Index));
                }
                catch (DuplicateDataException ex)
                {
                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = ex.Message,
                            Type = ResponseTypes.Danger
                        });
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to update store location.";
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
        public async Task<JsonResult> GetPagedStoreLocations([FromBody] StoreLocationListModel model)
        {
            try
            {
                var query = _mapper.Map<GetAllStoreLocationsByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("Title", "Address", "PhoneNumber", "DisplayOrder", "IsActive", "Id");

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllStoreLocationsByPagingQuery,
                    (IList<StoreLocation>, int, int)>(query);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                HttpUtility.HtmlEncode(item.Title),
                                HttpUtility.HtmlEncode(item.Address),
                                HttpUtility.HtmlEncode(item.PhoneNumber),
                                item.DisplayOrder.ToString(),
                                item.IsActive ? "Active" : "Inactive",
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get store location list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new StoreLocationDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Store location successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete store location.");
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to delete store location.",
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

