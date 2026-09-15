namespace MaleFashion.Web.Models.Home
{
    public class HomeBannerModel
    {
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = "#";
        public string CssModifier { get; set; } = string.Empty; // e.g., "banner__item--middle", "banner__item--last"
        public string ColumnClass { get; set; } = "col-lg-7"; // e.g., "col-lg-7 offset-lg-4", "col-lg-5", "col-lg-7"
    }

}
