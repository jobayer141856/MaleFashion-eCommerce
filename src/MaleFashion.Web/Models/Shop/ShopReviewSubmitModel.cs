using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Models.Shop
{
    public class ShopReviewSubmitModel
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Your name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string ReviewerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string ReviewerEmail { get; set; } = string.Empty;

        [Range(1, 5, ErrorMessage = "Please select a rating from 1 to 5 stars.")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Review title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Review comment is required.")]
        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string Comment { get; set; } = string.Empty;
    }
}

