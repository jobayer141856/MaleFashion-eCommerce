using MaleFashion.Domain.Utilities;

namespace MaleFashion.Web.Areas.Admin.Models.Catalogs
{
    public class ProductListModel : DataTables
    {
        public string? StockFilter { get; set; }
    }
}

