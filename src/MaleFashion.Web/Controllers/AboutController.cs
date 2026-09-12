using MaleFashion.Application.Contracts;
using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Web.Models.CMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers
{
    [Route("about")]
    [Route("about-us")]
    public class AboutController : Controller
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly ILogger<AboutController> _logger;

        public AboutController(
            IApplicationUnitOfWork unitOfWork,
            ILogger<AboutController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
        {
            try
            {
                var teamMembers = await _unitOfWork.TeamMemberRepository.GetAllAsync(cancellationToken);
                var partners = await _unitOfWork.PartnerClientRepository.GetAllAsync(cancellationToken);
                var testimonials = await _unitOfWork.TestimonialRepository.GetAllAsync(cancellationToken);

                var categoryCount = await _unitOfWork.CategoryRepository.GetCountAsync(null, cancellationToken);
                var partnerCount = await _unitOfWork.PartnerClientRepository.GetCountAsync(null, cancellationToken);

                var model = new AboutModel
                {
                    TeamMembers = (teamMembers != null && teamMembers.Count > 0)
                        ? teamMembers.OrderBy(t => t.CreatedAt).ToList()
                        : GetDefaultTeamMembers(),

                    Partners = (partners != null && partners.Count > 0)
                        ? partners.OrderBy(p => p.CreatedAt).ToList()
                        : GetDefaultPartners(),

                    FeaturedTestimonial = testimonials?.OrderByDescending(t => t.CreatedAt).FirstOrDefault() ?? GetDefaultTestimonial(),

                    Counters = new AboutCounters
                    {
                        ClientsCount = partnerCount > 0 ? partnerCount : 102,
                        CategoriesCount = categoryCount > 0 ? categoryCount : 30,
                        CountriesCount = 102,
                        HappyCustomerPercentage = 98
                    },

                    Features = GetDefaultFeatures()
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load About Us page data.");

                var fallbackModel = new AboutModel
                {
                    TeamMembers = GetDefaultTeamMembers(),
                    Partners = GetDefaultPartners(),
                    FeaturedTestimonial = GetDefaultTestimonial(),
                    Counters = new AboutCounters
                    {
                        ClientsCount = 102,
                        CategoriesCount = 30,
                        CountriesCount = 102,
                        HappyCustomerPercentage = 98
                    },
                    Features = GetDefaultFeatures()
                };

                return View(fallbackModel);
            }
        }

        private static List<AboutFeatureItem> GetDefaultFeatures()
        {
            return new List<AboutFeatureItem>
            {
                new()
                {
                    Title = "Who We Are ?",
                    Description = "Contextual advertising programs sometimes have strict policies that need to be adhered too. Let’s take Google as an example."
                },
                new()
                {
                    Title = "Who We Do ?",
                    Description = "In this digital generation where information can be easily obtained within seconds, business cards still have retained their importance."
                },
                new()
                {
                    Title = "Why Choose Us",
                    Description = "A two or three storey house is the ideal way to maximise the piece of earth on which our home sits, but for older or infirm people."
                }
            };
        }

        private static List<TeamMember> GetDefaultTeamMembers()
        {
            return new List<TeamMember>
            {
                new() { Name = "John Smith", Role = "Fashion Design", PhotoUrl = "malefashion/img/about/team-1.jpg" },
                new() { Name = "Christine Wise", Role = "C.E.O", PhotoUrl = "malefashion/img/about/team-2.jpg" },
                new() { Name = "Sean Robbins", Role = "Manager", PhotoUrl = "malefashion/img/about/team-3.jpg" },
                new() { Name = "Lucy Myers", Role = "Delivery", PhotoUrl = "malefashion/img/about/team-4.jpg" }
            };
        }

        private static List<PartnerClient> GetDefaultPartners()
        {
            return new List<PartnerClient>
            {
                new() { ClientName = "Partner 1", LogoUrl = "malefashion/img/clients/client-1.png" },
                new() { ClientName = "Partner 2", LogoUrl = "malefashion/img/clients/client-2.png" },
                new() { ClientName = "Partner 3", LogoUrl = "malefashion/img/clients/client-3.png" },
                new() { ClientName = "Partner 4", LogoUrl = "malefashion/img/clients/client-4.png" },
                new() { ClientName = "Partner 5", LogoUrl = "malefashion/img/clients/client-5.png" },
                new() { ClientName = "Partner 6", LogoUrl = "malefashion/img/clients/client-6.png" },
                new() { ClientName = "Partner 7", LogoUrl = "malefashion/img/clients/client-7.png" },
                new() { ClientName = "Partner 8", LogoUrl = "malefashion/img/clients/client-8.png" }
            };
        }

        private static Testimonial GetDefaultTestimonial()
        {
            return new Testimonial
            {
                AuthorName = "Augusta Schultz",
                AuthorProfession = "Fashion Design",
                AvatarUrl = "malefashion/img/about/testimonial-author.jpg",
                Quote = "“Going out after work? Take your butane curling iron with you to the office, heat it up, style your hair before you leave the office and you won’t have to make a trip back home.”"
            };
        }
    }
}

