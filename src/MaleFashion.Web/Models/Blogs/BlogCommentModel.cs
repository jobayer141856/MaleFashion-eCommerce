namespace MaleFashion.Web.Models.Blogs
{
    public class BlogCommentModel
    {
        public Guid BlogPostId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }
}
