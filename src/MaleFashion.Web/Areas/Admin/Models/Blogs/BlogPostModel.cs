using MaleFashion.Domain.Utilities;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace MaleFashion.Web.Areas.Admin.Models.Blogs
{
    public class BlogPostModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Blog title is required.")]
        [StringLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        [Display(Name = "Author Name")]
        [StringLength(150)]
        public string AuthorName { get; set; } = "Admin";

        [Display(Name = "Featured Image")]
        public IFormFile? FeaturedImage { get; set; }
        public string? ExistingImageUrl { get; set; }

        [Display(Name = "Tags (comma-separated)")]
        public string? TagNamesCsv { get; set; } // e.g. "Fashion, Trending, 2020"

        [Display(Name = "Publish Immediately")]
        public bool IsPublished { get; set; } = true;
    }

    public class BlogPostListModel : DataTables { }

    public class BlogCommentListModel : DataTables
    {
        public bool? StatusFilter { get; set; }
    }
}