using Cortex.Mediator;
using MaleFashion.Application.Features.Home.DTOs;
using MaleFashion.Application.Features.Home.Queries;
using MaleFashion.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<HomeController> _logger;

        public HomeController(
            IMediator mediator,
            ILogger<HomeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                var overview = await _mediator.SendQueryAsync<GetHomePageOverviewQuery, HomePageOverviewDto>(
                    new GetHomePageOverviewQuery(), cancellationToken);

                return View(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Home page overview.");
                return View(new HomePageOverviewDto());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

