using Cortex.Mediator;
using MaleFashion.Application.Features.Support.ContactInquiries.Commands;
using MaleFashion.Application.Features.Support.StoreLocations.Queries;
using MaleFashion.Domain.Entities.Support;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Models.ContactInquiries;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers.Support
{
    [Route("contact")]
    [Route("contacts")]
    [Route("contactinquiry")]
    public class ContactInquiryController : Controller
    {
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ContactInquiryController> _logger;

        public ContactInquiryController(
            IMediator mediator,
            UserManager<ApplicationUser> userManager,
            ILogger<ContactInquiryController> logger)
        {
            _mediator = mediator;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: /contact, /contacts, /contactinquiry
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            var locations = await _mediator.SendQueryAsync(new GetActiveStoreLocationsQuery(), cancellationToken);

            var model = new ContactInquiryModel
            {
                Locations = (locations != null && locations.Count > 0)
                    ? locations
                    : GetDefaultLocations()
            };

            // Pre-fill user data if logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var fullName = $"{user.FirstName} {user.LastName}".Trim();
                    model.Name = !string.IsNullOrWhiteSpace(fullName) ? fullName : (user.UserName ?? string.Empty);
                    model.Email = user.Email ?? string.Empty;
                }
            }

            return View(model);
        }

        // POST: /contact, /contacts, /contactinquiry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactInquiryModel model, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                var locations = await _mediator.SendQueryAsync(new GetActiveStoreLocationsQuery(), cancellationToken);
                model.Locations = (locations != null && locations.Count > 0)
                    ? locations
                    : GetDefaultLocations();
                return View(model);
            }

            try
            {
                var command = new ContactInquiryAddCommand
                {
                    Name = model.Name,
                    Email = model.Email,
                    Message = model.Message
                };

                await _mediator.SendCommandAsync(command, cancellationToken);
                TempData["SuccessMessage"] = "Thank you! Your message has been sent successfully. We will get back to you soon.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to submit contact inquiry.");
                ModelState.AddModelError(string.Empty, "An error occurred while sending your message. Please try again later.");
                var locations = await _mediator.SendQueryAsync(new GetActiveStoreLocationsQuery(), cancellationToken);
                model.Locations = (locations != null && locations.Count > 0)
                    ? locations
                    : GetDefaultLocations();
                return View(model);
            }
        }

        private static List<StoreLocation> GetDefaultLocations()
        {
            return new List<StoreLocation>
            {
                new()
                {
                    Title = "America",
                    Address = "195 E Parker Square Dr, Parker, CO 801",
                    PhoneNumber = "+43 982-314-0958",
                    IsActive = true
                },
                new()
                {
                    Title = "France",
                    Address = "109 Avenue Léon, 63 Clermont-Ferrand",
                    PhoneNumber = "+12 345-423-9893",
                    IsActive = true
                }
            };
        }
    }
}

