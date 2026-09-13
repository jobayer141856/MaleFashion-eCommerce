using Cortex.Mediator;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Application.Features.Blogs.Commands;
using MaleFashion.Application.Features.Blogs.Queries;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Extensions;
using MaleFashion.Web.Areas.Admin.Models;
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
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,BlogEditor")]
    public class BlogPostsController : Controller
    {
        private readonly ILogger<BlogPostsController> _logger;
        private readonly IMediator _mediator;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;

        public BlogPostsController(
            ILogger<BlogPostsController> logger,
            IMediator mediator,
            IMapper mapper,
            IFileStorageService fileStorageService)
        {
            _logger = logger;
            _mediator = mediator;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
        }

        public IActionResult Index() => View();

        public IActionResult Create() => View(new BlogPostModel { IsPublished = true, AuthorName = User.Identity?.Name ?? "Admin" });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BlogPostModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string? imageName = null;
                    if (model.FeaturedImage != null)
                    {
                        await using var stream = model.FeaturedImage.OpenReadStream();
                        imageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.FeaturedImage.FileName,
                            "blogs",
                            cancellationToken);
                    }

                    var command = new BlogPostAddCommand
                    {
                        Title = model.Title,
                        Content = model.Content,
                        AuthorName = string.IsNullOrWhiteSpace(model.AuthorName) ? "Admin" : model.AuthorName.Trim(),
                        FeaturedImageUrl = imageName ?? string.Empty,
                        IsPublished = model.IsPublished,
                        Tags = model.TagNamesCsv?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new()
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    TempData.Put(Constants.ResponseTempKey, new ResponseModel
                    {
                        Message = "Blog post successfully created.",
                        Type = ResponseTypes.Success
                    });

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Failed to create blog post.";
                    _logger.LogError(ex, errorMessage);
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = errorMessage, Type = ResponseTypes.Danger });
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Update(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _mediator.SendQueryAsync(new GetBlogPostByIdQuery { Id = id }, cancellationToken);
                if (post == null)
                {
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = "Blog post not found.", Type = ResponseTypes.Danger });
                    return RedirectToAction(nameof(Index));
                }

                var model = new BlogPostModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    AuthorName = post.AuthorName,
                    ExistingImageUrl = post.FeaturedImageUrl,
                    IsPublished = post.IsPublished,
                    TagNamesCsv = string.Join(", ", post.Tags.Select(t => t.Name))
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load blog post.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(BlogPostModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var post = await _mediator.SendQueryAsync(new GetBlogPostByIdQuery { Id = model.Id }, cancellationToken);
                    if (post == null)
                    {
                        TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = "Blog post does not exist.", Type = ResponseTypes.Danger });
                        return RedirectToAction(nameof(Index));
                    }

                    var oldImageName = post.FeaturedImageUrl;
                    var imageName = oldImageName;

                    if (model.FeaturedImage != null)
                    {
                        await using var stream = model.FeaturedImage.OpenReadStream();
                        imageName = await _fileStorageService.SaveImageAsync(
                            stream,
                            model.FeaturedImage.FileName,
                            "blogs",
                            cancellationToken);
                    }

                    var command = new BlogPostUpdateCommand
                    {
                        Id = model.Id,
                        Title = model.Title,
                        Content = model.Content,
                        AuthorName = model.AuthorName,
                        FeaturedImageUrl = imageName,
                        IsPublished = model.IsPublished,
                        Tags = model.TagNamesCsv?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList() ?? new()
                    };

                    await _mediator.SendCommandAsync(command, cancellationToken);

                    // Delete old image if replaced
                    if (model.FeaturedImage != null && !string.IsNullOrEmpty(oldImageName) && oldImageName != imageName)
                    {
                        try
                        {
                            await _fileStorageService.DeleteImageAsync(oldImageName, "blogs", cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to delete old blog image.");
                        }
                    }

                    TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = "Blog post updated successfully.", Type = ResponseTypes.Success });
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update blog post.");
                    TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = "Failed to update blog post.", Type = ResponseTypes.Danger });
                }
            }

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedBlogPosts([FromBody] BlogPostListModel model)
        {
            try
            {
                var query = _mapper.Map<GetAllBlogPostsByPagingQuery>(model);
                query.SearchText = model.Search.Value;
                query.SortText = model.FormatSortExpression("FeaturedImageUrl", "Title", "Content", "AuthorName", "PublishedDate", "IsPublished", "Id");

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllBlogPostsByPagingQuery,
                    (System.Collections.Generic.IList<Domain.Entities.Blogs.BlogPost>, int, int)>(query);

                // Helper to strip HTML and truncate to max 20 chars
                string TruncateContent(string? content, int maxLength = 20)
                {
                    if (string.IsNullOrWhiteSpace(content)) return "-";
                    var plainText = System.Text.RegularExpressions.Regex.Replace(content, "<.*?>", string.Empty).Trim();
                    return plainText.Length <= maxLength ? plainText : plainText.Substring(0, maxLength) + "...";
                }

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            select new string[]
                            {
                                !string.IsNullOrEmpty(item.FeaturedImageUrl)
                                    ? $"/uploads/blogs/{HttpUtility.HtmlEncode(item.FeaturedImageUrl)}"
                                    : "/sneat/assets/img/elements/1.jpg",
                                HttpUtility.HtmlEncode(item.Title),
                                HttpUtility.HtmlEncode(TruncateContent(item.Content, 20)), // Max 20 chars
                                HttpUtility.HtmlEncode(item.AuthorName),
                                item.PublishedDate.ToString("MMM dd, yyyy"),
                                item.IsPublished ? "Published" : "Draft",
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load blog post list.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpPost]
        public async Task<IActionResult> TogglePublish(Guid id, CancellationToken cancellationToken)
        {
            var isPublished = await _mediator.SendCommandAsync(new BlogPostTogglePublishCommand { Id = id }, cancellationToken);
            return Json(new { success = true, isPublished });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var post = await _mediator.SendQueryAsync(new GetBlogPostByIdQuery { Id = id }, cancellationToken);
                if (post != null && !string.IsNullOrEmpty(post.FeaturedImageUrl))
                {
                    try
                    {
                        await _fileStorageService.DeleteImageAsync(post.FeaturedImageUrl, "blogs", cancellationToken);
                    }
                    catch { }
                }

                await _mediator.SendCommandAsync(new BlogPostDeleteCommand { Id = id }, cancellationToken);
                TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = "Blog post deleted successfully.", Type = ResponseTypes.Success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete blog post.");
                TempData.Put(Constants.ResponseTempKey, new ResponseModel { Message = "Failed to delete blog post.", Type = ResponseTypes.Danger });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}