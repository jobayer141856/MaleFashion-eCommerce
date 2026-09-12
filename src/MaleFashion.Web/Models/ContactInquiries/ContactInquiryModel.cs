using MaleFashion.Domain.Entities.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.ContactInquiries
{
    public class ContactInquiryModel
    {
        public IList<StoreLocation> Locations { get; set; } = new List<StoreLocation>();

        [Required(ErrorMessage = "Please enter your name.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your email.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter your message.")]
        [StringLength(3000, ErrorMessage = "Message cannot exceed 3000 characters.")]
        public string Message { get; set; } = string.Empty;
    }
}

