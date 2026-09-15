namespace MaleFashion.Web.Models.Home
{
    public class HomeBlogCardModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string TargetUrl { get; set; } = "#";
    }
}
