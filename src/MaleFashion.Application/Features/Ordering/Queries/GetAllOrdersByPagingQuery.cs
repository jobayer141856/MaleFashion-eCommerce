using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.Ordering;
using MaleFashion.Domain.Enums;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.Ordering.Queries
{
    public class GetAllOrdersByPagingQuery : IQuery<(IList<Order>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
        public OrderStatus? StatusFilter { get; set; }
        public PaymentStatus? PaymentStatusFilter { get; set; }
        public PaymentMethod? PaymentMethodFilter { get; set; }
    }
}

