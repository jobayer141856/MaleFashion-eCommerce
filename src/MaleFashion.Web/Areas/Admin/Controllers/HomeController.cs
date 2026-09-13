using Cortex.Mediator;
using MaleFashion.Application.Features.Dashboard.DTOs;
using MaleFashion.Application.Features.Dashboard.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IMediator mediator, ILogger<HomeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var dashboardData = await _mediator.SendQueryAsync<GetAdminDashboardOverviewQuery, AdminDashboardDto>(
                    new GetAdminDashboardOverviewQuery(), cancellationToken);

                return View(dashboardData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load admin dashboard overview.");
                return View(new AdminDashboardDto());
            }
        }
    }
}

