using MaleFashion.Domain.Utilities;

namespace MaleFashion.Web.Areas.Admin.Models.Reviews
{
    public class ProductReviewListModel : DataTables
    {
        public bool? StatusFilter { get; set; }
        public int? RatingFilter { get; set; }
        public bool? VerifiedOnly { get; set; }
    }
}

