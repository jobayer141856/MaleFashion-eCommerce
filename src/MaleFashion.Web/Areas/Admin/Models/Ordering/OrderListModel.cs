using MaleFashion.Domain.Enums;
using MaleFashion.Domain.Utilities;

namespace MaleFashion.Web.Areas.Admin.Models.Ordering
{
    public class OrderListModel : DataTables
    {
        public OrderStatus? StatusFilter { get; set; }
        public PaymentStatus? PaymentStatusFilter { get; set; }
        public PaymentMethod? PaymentMethodFilter { get; set; }
    }
}

