using Cortex.Mediator;
using MaleFashion.Application.Features.Blogs.Commands;
using MaleFashion.Application.Features.Blogs.Queries;
using MaleFashion.Web.Areas.Admin.Models.Blogs;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Blogs
{
    [Area("Admin"), Authorize(Roles = "Admin,SuperAdmin")]
    public class BlogCommentsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;

        public BlogCommentsController(IMediator mediator, IMapper mapper)
        {
            _mediator = mediator;
            _mapper = mapper;
        }

        public IActionResult Index() => View();

        [HttpPost]
        public async Task<JsonResult> GetPagedComments([FromBody] BlogCommentListModel model)
        {
            var query = _mapper.Map<GetAllBlogCommentsByPagingQuery>(model);
            query.SearchText = model.Search.Value;
            query.StatusFilter = model.StatusFilter;
            query.SortText = model.FormatSortExpression("BlogPost.Title", "Name", "CommentText", "CreatedAt", "IsApproved", "Id");

            var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllBlogCommentsByPagingQuery, (System.Collections.Generic.IList<Domain.Entities.Blogs.BlogComment>, int, int)>(query);

            var response = new
            {
                recordsTotal = total,
                recordsFiltered = totalDisplay,
                data = items.Select(c => new string[]
                {
                    HttpUtility.HtmlEncode(c.BlogPost?.Title ?? "N/A"),
                    HttpUtility.HtmlEncode(c.Name),
                    HttpUtility.HtmlEncode(c.CommentText),
                    c.CreatedAt.ToString("MMM dd, yyyy"),
                    c.IsApproved ? "Approved" : "Pending",
                    c.Id.ToString()
                }).ToArray()
            };

            return Json(response);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleApproval(Guid id, CancellationToken cancellationToken)
        {
            var isApproved = await _mediator.SendCommandAsync(new BlogCommentToggleApproveCommand { Id = id }, cancellationToken);
            return Json(new { success = true, isApproved });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            await _mediator.SendCommandAsync(new BlogCommentDeleteCommand { Id = id }, cancellationToken);
            return Json(new { success = true });
        }
    }
}