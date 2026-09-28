using MaleFashion.Domain.Utilities;

namespace MaleFashion.Web.Areas.Admin.Models.Catalogs
{
    public class ProductListSPModel : DataTables
    {
        public string? Name { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
    }
}
