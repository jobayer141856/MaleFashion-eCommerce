namespace MaleFashion.Web.Models.Shop
{
    public class ProductVariantItemDto
    {
        public Guid Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public string ColorName { get; set; } = string.Empty;
        public string ColorHex { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal? Price { get; set; }
        public string Sku { get; set; } = string.Empty;
    }
}
