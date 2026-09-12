using MaleFashion.Domain.Utilities;

namespace MaleFashion.Web.Areas.Admin.Models.Carts
{
    public class ActiveCartListModel : DataTables
    {
        public string? CartFilter { get; set; }
    }
}

