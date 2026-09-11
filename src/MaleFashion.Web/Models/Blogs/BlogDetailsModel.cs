using MaleFashion.Domain.Entities.Blogs;

namespace MaleFashion.Web.Models.Blogs
{
    public class BlogDetailsModel
    {
        public BlogPost Post { get; set; } = default!;
        public BlogPost? PreviousPost { get; set; }
        public BlogPost? NextPost { get; set; }
        public BlogCommentModel CommentInput { get; set; } = new();
    }
}
