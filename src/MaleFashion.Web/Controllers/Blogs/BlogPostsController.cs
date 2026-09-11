using Cortex.Mediator;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Blogs.Commands;
using MaleFashion.Infrastructure.Identity;
using MaleFashion.Web.Models.Blogs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers
{
    public class BlogPostsController : Controller
    {
        private readonly IApplicationUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly UserManager<ApplicationUser> _userManager;

        public BlogPostsController(
            IApplicationUnitOfWork unitOfWork,
            IMediator mediator,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _userManager = userManager;
        }

        // GET: /Blog
        public async Task<IActionResult> Index(int page = 1, string? tag = null, string? search = null, CancellationToken cancellationToken = default)
        {
            const int pageSize = 9;
            var (posts, total) = await _unitOfWork.BlogPostRepository.GetPublishedBlogPosts(page, pageSize, tag, search, cancellationToken);

            var model = new BlogModel
            {
                Posts = posts,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(total / (double)pageSize),
                SelectedTag = tag,
                SearchTerm = search
            };

            return View(model);
        }

        // GET: /Blog/Details/{id}
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            var post = await _unitOfWork.BlogPostRepository.GetBlogPostDetailsById(id, cancellationToken);
            if (post == null || !post.IsPublished) return NotFound();

            var (prev, next) = await _unitOfWork.BlogPostRepository.GetAdjacentPosts(post.PublishedDate, post.Id, cancellationToken);

            var model = new BlogDetailsModel
            {
                Post = post,
                PreviousPost = prev,
                NextPost = next
            };

            // Pre-fill user data if logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CommentInput.Name = $"{user.FirstName} {user.LastName}";
                    model.CommentInput.Email = user.Email ?? string.Empty;
                    model.CommentInput.Phone = user.PhoneNumber;
                }
            }

            return View(model);
        }

        // POST: /Blog/AddComment
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(BlogDetailsModel model, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrWhiteSpace(model.CommentInput.CommentText) &&
                !string.IsNullOrWhiteSpace(model.CommentInput.Name) &&
                !string.IsNullOrWhiteSpace(model.CommentInput.Email))
            {
                Guid? userId = null;
                if (User.Identity?.IsAuthenticated == true)
                {
                    var user = await _userManager.GetUserAsync(User);
                    if (user != null) userId = Guid.Parse(user.Id.ToString());
                }

                var command = new BlogCommentAddCommand
                {
                    BlogPostId = model.CommentInput.BlogPostId,
                    UserId = userId,
                    Name = model.CommentInput.Name,
                    Email = model.CommentInput.Email,
                    Phone = model.CommentInput.Phone,
                    CommentText = model.CommentInput.CommentText
                };

                await _mediator.SendCommandAsync(command, cancellationToken);
                TempData["CommentSuccess"] = "Thank you! Your comment has been submitted and is awaiting administrator approval.";
            }

            return RedirectToAction(nameof(Details), new { id = model.CommentInput.BlogPostId });
        }
    }
}