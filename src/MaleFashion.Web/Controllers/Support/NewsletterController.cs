using Cortex.Mediator;
using MaleFashion.Application.Features.Support.NewsletterSubscribers.Commands;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Models.Support;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers.Support
{
    [Route("newsletter")]
    public class NewsletterController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<NewsletterController> _logger;

        public NewsletterController(
            IMediator mediator,
            ILogger<NewsletterController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("subscribe")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe([FromForm] NewsletterSubscribeModel model, CancellationToken cancellationToken)
        {
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest"
                         || Request.Headers.Accept.ToString().Contains("application/json");

            if (!ModelState.IsValid)
            {
                var errorMsg = "Please enter a valid email address.";
                if (isAjax)
                {
                    return Json(new { success = false, message = errorMsg });
                }

                TempData["NewsletterError"] = errorMsg;
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }

            try
            {
                var command = new NewsletterSubscribeCommand
                {
                    Email = model.Email
                };

                var result = await _mediator.SendCommandAsync(command, cancellationToken);

                if (isAjax)
                {
                    return Json(new
                    {
                        success = result.Success,
                        message = result.Message,
                        isReactivated = result.IsReactivated
                    });
                }

                if (result.Success)
                    TempData["NewsletterSuccess"] = result.Message;
                else
                    TempData["NewsletterError"] = result.Message;

                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing newsletter subscription for email {Email}", model.Email);
                const string fallbackError = "An unexpected error occurred. Please try again later.";

                if (isAjax)
                {
                    return Json(new { success = false, message = fallbackError });
                }

                TempData["NewsletterError"] = fallbackError;
                return Redirect(Request.Headers["Referer"].ToString() ?? "/");
            }
        }
    }
}

