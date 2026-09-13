using Cortex.Mediator;
using MaleFashion.Application.Features.Reviews.Commands;
using MaleFashion.Application.Features.Reviews.Queries;
using MaleFashion.Domain.Entities.Reviews;
using MaleFashion.Domain.Utilities;
using MaleFashion.Web.Areas.Admin.Models.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace MaleFashion.Web.Areas.Admin.Controllers.Reviews
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin,SupportStaff")]
    public class ReviewsController : Controller
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ReviewsController> _logger;

        public ReviewsController(IMediator mediator, ILogger<ReviewsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<IActionResult> Index(CancellationToken cancellationToken)
        {
            try
            {
                var (total, pending, approved, avgRating) = await _mediator.SendQueryAsync<GetProductReviewStatsQuery,
                    (int, int, int, double)>(new GetProductReviewStatsQuery(), cancellationToken);

                ViewBag.TotalReviews = total;
                ViewBag.PendingReviews = pending;
                ViewBag.ApprovedReviews = approved;
                ViewBag.AverageRating = avgRating;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch product review stats.");
                ViewBag.TotalReviews = 0;
                ViewBag.PendingReviews = 0;
                ViewBag.ApprovedReviews = 0;
                ViewBag.AverageRating = 0.0;
            }

            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GetPagedReviews([FromBody] ProductReviewListModel model, CancellationToken cancellationToken)
        {
            try
            {
                var query = new GetAllProductReviewsByPagingQuery
                {
                    PageIndex = model.PageIndex,
                    PageSize = model.PageSize,
                    SearchText = model.Search.Value ?? string.Empty,
                    SortText = model.FormatSortExpression("Product.Name", "ReviewerName", "Rating", "Title", "CreatedAt", "IsApproved", "Id"),
                    StatusFilter = model.StatusFilter,
                    RatingFilter = model.RatingFilter,
                    VerifiedOnly = model.VerifiedOnly
                };

                var (items, total, totalDisplay) = await _mediator.SendQueryAsync<GetAllProductReviewsByPagingQuery,
                    (IList<ProductReview>, int, int)>(query, cancellationToken);

                var response = new
                {
                    recordsTotal = total,
                    recordsFiltered = totalDisplay,
                    data = (from item in items
                            let imgUrl = item.Product?.Images?.FirstOrDefault()?.ImageUrl
                            let imgTag = !string.IsNullOrEmpty(imgUrl)
                                ? $"/uploads/products/{HttpUtility.HtmlEncode(imgUrl)}"
                                : "/img/product/product-1.jpg"
                            let productHtml = $@"<div class='d-flex align-items-center'>
                                <img src='{imgTag}' class='rounded me-2' style='width:45px;height:45px;object-fit:cover;' />
                                <div>
                                    <span class='fw-semibold d-block'>{HttpUtility.HtmlEncode(item.Product?.Name ?? "N/A")}</span>
                                    <small class='text-muted'>SKU: {HttpUtility.HtmlEncode(item.Product?.Sku ?? "N/A")}</small>
                                </div>
                            </div>"
                            let verifiedBadge = item.IsVerifiedPurchase
                                ? "<span class='badge bg-label-success ms-1' title='Verified Purchase'><i class='bx bx-check-shield'></i> Verified</span>"
                                : "<span class='badge bg-label-secondary ms-1' title='Unverified'>Unverified</span>"
                            let reviewerHtml = $@"<div>
                                <span class='fw-semibold d-block'>{HttpUtility.HtmlEncode(item.ReviewerName)} {verifiedBadge}</span>
                                <small class='text-muted'>{HttpUtility.HtmlEncode(item.ReviewerEmail)}</small>
                            </div>"
                            let stars = RenderStarsHtml(item.Rating)
                            let commentExcerpt = item.Comment.Length > 80
                                ? $"{HttpUtility.HtmlEncode(item.Comment.Substring(0, 80))}..."
                                : HttpUtility.HtmlEncode(item.Comment)
                            let reviewContentHtml = $@"<div>
                                <strong class='d-block text-dark'>{HttpUtility.HtmlEncode(item.Title)}</strong>
                                <small class='text-muted'>{commentExcerpt}</small>
                            </div>"
                            let statusText = item.IsApproved ? "Approved" : "Pending"
                            select new string[]
                            {
                                productHtml,
                                reviewerHtml,
                                stars,
                                reviewContentHtml,
                                item.CreatedAt.ToString("MMM dd, yyyy"),
                                statusText,
                                item.Id.ToString()
                            }).ToArray()
                };

                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get paged product reviews.");
                return Json(DataTables.EmptyResult);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetDetails(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var review = await _mediator.SendQueryAsync<GetProductReviewByIdQuery, ProductReview?>(
                    new GetProductReviewByIdQuery(id), cancellationToken);

                if (review == null)
                    return Json(new { success = false, message = "Review not found." });

                var imgUrl = review.Product?.Images?.FirstOrDefault()?.ImageUrl;
                var model = new ProductReviewViewModel
                {
                    Id = review.Id,
                    ProductId = review.ProductId,
                    ProductName = review.Product?.Name ?? "N/A",
                    ProductImageUrl = !string.IsNullOrEmpty(imgUrl) ? $"/uploads/products/{imgUrl}" : "/img/product/product-1.jpg",
                    CustomerId = review.CustomerId,
                    ReviewerName = review.ReviewerName,
                    ReviewerEmail = review.ReviewerEmail,
                    Rating = review.Rating,
                    Title = review.Title,
                    Comment = review.Comment,
                    IsVerifiedPurchase = review.IsVerifiedPurchase,
                    IsApproved = review.IsApproved,
                    HelpfulCount = review.HelpfulCount,
                    CreatedAt = review.CreatedAt,
                    UpdatedAt = review.UpdatedAt
                };

                return Json(new { success = true, review = model });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get review details for ID {ReviewId}", id);
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ToggleApproval(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var isApproved = await _mediator.SendCommandAsync(new ProductReviewToggleApproveCommand { Id = id }, cancellationToken);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new
                    {
                        success = true,
                        isApproved,
                        message = isApproved ? "Review approved and published to storefront." : "Review returned to pending moderation."
                    });
                }

                TempData["SuccessMessage"] = isApproved ? "Review approved and published." : "Review returned to pending moderation.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to toggle review approval for ID {ReviewId}", id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = ex.Message });

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                await _mediator.SendCommandAsync(new ProductReviewDeleteCommand { Id = id }, cancellationToken);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = true, message = "Product review deleted successfully." });

                TempData["SuccessMessage"] = "Product review deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete review with ID {ReviewId}", id);
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false, message = ex.Message });

                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        private static string RenderStarsHtml(int rating)
        {
            var sb = new StringBuilder();
            sb.Append("<div class='d-flex align-items-center text-warning' title='").Append(rating).Append(" / 5'>");
            for (int i = 1; i <= 5; i++)
            {
                if (i <= rating)
                    sb.Append("<i class='bx bxs-star'></i>");
                else
                    sb.Append("<i class='bx bx-star text-secondary'></i>");
            }
            sb.Append("<span class='ms-1 text-muted small'>(").Append(rating).Append("/5)</span>");
            sb.Append("</div>");
            return sb.ToString();
        }
    }
}
