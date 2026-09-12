using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.CMS
{
    public class AboutFeatureItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class AboutCounters
    {
        public int ClientsCount { get; set; }
        public int CategoriesCount { get; set; }
        public int CountriesCount { get; set; }
        public int HappyCustomerPercentage { get; set; }
    }

    public class AboutModel
    {
        public IList<AboutFeatureItem> Features { get; set; } = new List<AboutFeatureItem>();
        public AboutCounters Counters { get; set; } = new();
        public Testimonial? FeaturedTestimonial { get; set; }
        public IList<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public IList<PartnerClient> Partners { get; set; } = new List<PartnerClient>();
    }
}

