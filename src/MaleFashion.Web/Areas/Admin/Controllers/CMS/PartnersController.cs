using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Exceptions;
using MaleFashion.Application.Features.CMS.Partners.Commands;
using MaleFashion.Application.Features.CMS.Partners.Queries;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.CMS
{
    [Area("Admin")]
    public class PartnersController : Controller
    {
        private readonly ILogger<PartnersController> _logger;
        private readonly IMediator _mediator;
        private readonly IFileStorageService _fileStorageService;

        public PartnersController(
            ILogger<PartnersController> logger,
            IMediator mediator,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _mediator = mediator;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Create()
        {
            var model = new PartnerClientModel();
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PartnerClientModel model, CancellationToken cancellationToken)
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
                            "partners",
                            cancellationToken);
                    }

                    var command = new PartnerClientAddCommand
                    {
                        ClientName = model.ClientName,
                        LogoUrl = logoUrl
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Partner client successfully added.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to add partner client.";
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

        public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetPartnerClientByIdQuery { Id = id };
                var partner = await _mediator.SendQueryAsync(query, cancellationToken);
                if (partner == null)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Partner client not found.",
                        Type = ResponseTypes.Danger
                    });
                    return RedirectToAction(nameof(Index));
                }

                var model = new PartnerClientModel
                {
                    Id = partner.Id,
                    ClientName = partner.ClientName,
                    LogoUrl = partner.LogoUrl
                };

                return View(model);
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to load partner client.";
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
        public async Task<IActionResult> Update(PartnerClientModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var query = new GetPartnerClientByIdQuery { Id = model.Id };
                    var existing = await _mediator.SendQueryAsync(query, cancellationToken);
                    if (existing == null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel
                        {
                            Message = "Partner client does not exist.",
                            Type = ResponseTypes.Danger
                        });
                        return RedirectToAction(nameof(Index));
                    }

                    var oldLogoUrl = existing.LogoUrl;
                    var logoUrl = oldLogoUrl;

                    if (model.Logo != null)
                    {
                        await using var stream = model.Logo.OpenReadStream();
                        logoUrl = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.Logo.FileName,
                            "partners",
                            cancellationToken);
                    }

                    var command = new PartnerClientUpdateCommand
                    {
                        Id = model.Id,
                        ClientName = model.ClientName,
                        LogoUrl = logoUrl
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    // If a new logo was uploaded, clean up the old one
                    if (model.Logo != null && !string.IsNullOrEmpty(oldLogoUrl) && oldLogoUrl != logoUrl)
                    {
                        try
                        {
                            await _fileStorageService.DeleteImageAsync(oldLogoUrl, "partners", cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to delete old partner logo for ID: {Id}", model.Id);
                        }
                    }

                    TempData.Put(Constants.ResponseTempKey,
                        new ResponseModel
                        {
                            Message = "Partner client successfully updated.",
                            Type = ResponseTypes.Success
                        });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to update partner client.";
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
        public async Task<JsonResult> GetPagedPartners([FromBody] PartnerClientListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllPartnersByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("LogoUrl", "ClientName", "CreatedAt", "Id")
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllPartnersByPagingQuery,
                    (IList<PartnerClient>, int, int)>(query, cancellationToken);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                !string.IsNullOrEmpty(item.LogoUrl)
                                    ? $"/uploads/partners/{HttpUtility.HtmlEncode(item.LogoUrl)}"
                                    : "/img/clients/client-1.png",
                                HttpUtility.HtmlEncode(item.ClientName),
                                item.CreatedAt.ToString("MMM dd, yyyy"),
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get partner client list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new PartnerClientDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Partner client successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (NotFoundException nfex)
            {
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = nfex.Message,
                        Type = ResponseTypes.Danger
                    });
            }
            catch (Exception ex)
            {
                const string errorMessage = "Failed to delete partner client.";
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
    }
}
