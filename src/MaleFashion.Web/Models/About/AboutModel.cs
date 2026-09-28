using MaleFashion.Domain.Entities.CMS;
using MaleFashion.Web.Models.About;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.CMS
{
  
    public class AboutModel
    {
        public IList<AboutFeatureItem> Features { get; set; } = new List<AboutFeatureItem>();
        public AboutCounters Counters { get; set; } = new();
        public Testimonial? FeaturedTestimonial { get; set; }
        public IList<TeamMember> TeamMembers { get; set; } = new List<TeamMember>();
        public IList<PartnerClient> Partners { get; set; } = new List<PartnerClient>();
    }
}

