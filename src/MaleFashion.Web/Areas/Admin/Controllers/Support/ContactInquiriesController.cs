using Cortex.Mediator;
using MaleFashion.Application.Features.Support.ContactInquiries.Commands;
using MaleFashion.Application.Features.Support.ContactInquiries.Queries;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
using MaleFashion.Web.Areas.Admin.Models.Support;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin,SuperAdmin,SupportStaff")]
    public class ContactInquiriesController : Controller
    {
        private readonly ILogger<ContactInquiriesController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public ContactInquiriesController(
            ILogger<ContactInquiriesController> logger,
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

        [HttpPost]
        public async Task<JsonResult> GetPagedContactInquiries([FromBody] ContactInquiryListModel model)
        {
            try
            {
                var query = _mapper.Map<GetAllContactInquiriesByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("Name", "Email", "Message", "SubmittedAt", "IsResolved", "Id");

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllContactInquiriesByPagingQuery,
                    (IList<ContactInquiry>, int, int)>(query);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                HttpUtility.HtmlEncode(item.Name),
                                HttpUtility.HtmlEncode(item.Email),
                                HttpUtility.HtmlEncode(item.Message.Length > 80 ? item.Message.Substring(0, 80) + "..." : item.Message),
                                item.SubmittedAt.ToString("yyyy-MM-dd HH:mm"),
                                item.IsResolved ? "Resolved" : "Pending",
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get contact inquiries list");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid id)
        {
            try
            {
                var query = new GetContactInquiryByIdQuery { Id = id };
                var inquiry = await _mediator.SendQueryAsync(query);
                if (inquiry == null)
                    return NotFound(new { message = "Inquiry not found" });

                return Json(new
                {
                    id = inquiry.Id,
                    name = inquiry.Name,
                    email = inquiry.Email,
                    message = inquiry.Message,
                    submittedAt = inquiry.SubmittedAt.ToString("f"),
                    isResolved = inquiry.IsResolved
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get inquiry details.");
                return BadRequest(new { message = "Failed to load inquiry details." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleResolved(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ContactInquiryToggleResolvedCommand { Id = id };
                var isResolved = await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = $"Inquiry successfully marked as {(isResolved ? "Resolved" : "Pending")}.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle inquiry status.");
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to update inquiry status.",
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var command = new ContactInquiryDeleteCommand { Id = id };
                await _mediator.SendCommandAsync(command, cancellationToken);

                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Contact message successfully deleted.",
                        Type = ResponseTypes.Success
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete contact inquiry.");
                TempData.Put(Constants.ResponseTempKey,
                    new ResponseModel
                    {
                        Message = "Failed to delete contact message.",
                        Type = ResponseTypes.Danger
                    });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

