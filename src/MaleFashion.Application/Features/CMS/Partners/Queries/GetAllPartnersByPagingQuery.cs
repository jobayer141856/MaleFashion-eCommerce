using Cortex.Mediator.Queries;
using MaleFashion.Domain.Entities.CMS;
using System.Collections.Generic;

namespace MaleFashion.Application.Features.CMS.Partners.Queries
{
    public class GetAllPartnersByPagingQuery : IQuery<(IList<PartnerClient>, int, int)>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public string SearchText { get; set; } = string.Empty;
        public string SortText { get; set; } = string.Empty;
    }
}

