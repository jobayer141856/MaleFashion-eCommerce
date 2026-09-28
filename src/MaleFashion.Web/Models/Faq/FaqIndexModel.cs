using System.Collections.Generic;

namespace MaleFashion.Web.Models.Faq
{
    public class FaqIndexModel
    {
        public string? SelectedCategory { get; set; }
        public string? SearchQuery { get; set; }
        public List<FaqCategoryModel> Categories { get; set; } = new();
        public List<FaqItemModel> FaqItems { get; set; } = new();
    }
}

