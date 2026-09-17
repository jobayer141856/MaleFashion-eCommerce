using System.Collections.Generic;

namespace MaleFashion.Web.Models.Faq
{
    public class FaqItemModel
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Question { get; set; } = string.Empty;
        public string Answer { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class FaqCategoryModel
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class FaqIndexModel
    {
        public string? SelectedCategory { get; set; }
        public string? SearchQuery { get; set; }
        public List<FaqCategoryModel> Categories { get; set; } = new();
        public List<FaqItemModel> FaqItems { get; set; } = new();
    }
}

