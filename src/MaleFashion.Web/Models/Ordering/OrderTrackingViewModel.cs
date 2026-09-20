using MaleFashion.Domain.Entities.Ordering;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Ordering
{
    public class OrderTrackingViewModel
    {
        [Required(ErrorMessage = "Order Number is required")]
        [Display(Name = "Order Number")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        public Order? Order { get; set; }
        public bool Searched { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

