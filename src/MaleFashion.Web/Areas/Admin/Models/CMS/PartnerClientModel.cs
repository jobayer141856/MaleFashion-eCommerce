using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.CMS
{
    public class PartnerClientModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Client / Partner name is required.")]
        [StringLength(150, ErrorMessage = "Client name cannot exceed 150 characters.")]
        [Display(Name = "Client / Partner Name")]
        public string ClientName { get; set; } = string.Empty;

        [Display(Name = "Partner Logo")]
        public IFormFile? Logo { get; set; }

        public string? LogoUrl { get; set; }
    }
}

