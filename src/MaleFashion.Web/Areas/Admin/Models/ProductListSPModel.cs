using MaleFashion.Domain.Utilities;

namespace Demo.Web.Areas.Admin.Models
{
    public class ProductListSPModel : DataTables
    {
        public string Name { get; set; }
        public double? PriceFrom { get; set; }
        public double? PriceTo { get; set; }
    }
}
