using MaleFashion.Domain.Entities.Blogs;
using System;
using System.Collections.Generic;

namespace MaleFashion.Web.Models.Blogs
{
    public class BlogModel
    {
        public IList<BlogPost> Posts { get; set; } = new List<BlogPost>();
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string? SelectedTag { get; set; }
        public string? SearchTerm { get; set; }
    }

    
}