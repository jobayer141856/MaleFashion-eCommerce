namespace MaleFashion.Web.Models.Shop
{
    public class CategoryFilterItem
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int ProductCount { get; set; }
    }
}
