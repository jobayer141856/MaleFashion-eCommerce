using Cortex.Mediator;
using MaleFashion.Application.Contracts;
using MaleFashion.Application.Features.Catalogs.Products.DTOs;
using MaleFashion.Application.Features.Catalogs.Products.Queries;
using MaleFashion.Domain.Entities.Catalogs;
using MaleFashion.Domain.Entities.Reviews;
using MaleFashion.Web.Models.Shop;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers.Shop
{
    public class ShopController : Controller
    {
        private readonly IMediator _mediator;
        private readonly IApplicationUnitOfWork _unitOfWork;

        public ShopController(
            IMediator mediator,
            IApplicationUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        // GET: /Shop
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] GetShopProductsQuery query, CancellationToken cancellationToken)
        {
            // 1. Fetch products via GetShopProducts Stored Procedure using SqlUtility
            var (products, total, totalDisplay) = await _mediator.SendQueryAsync<GetShopProductsQuery, (IList<ShopProductItemDto>, int, int)>(
                query, cancellationToken);

            // 2. Fetch Category, Brand & Tag filters
            var categories = await _unitOfWork.CategoryRepository.GetAllAsync(cancellationToken);
            var brands = await _unitOfWork.BrandRepository.GetAllAsync(cancellationToken);
            var categoryCounts = await _unitOfWork.CategoryRepository.GetProductCountsByCategoryAsync(cancellationToken);
            var dynamicTags = await _unitOfWork.ProductRepository.GetAllDistinctTagNamesAsync(cancellationToken);

            var model = new ShopIndexModel
            {
                Products = products,
                Total = total,
                TotalDisplay = totalDisplay,
                PageIndex = query.PageIndex < 1 ? 1 : query.PageIndex,
                PageSize = query.PageSize < 1 ? 12 : query.PageSize,
                OrderBy = query.OrderBy ?? "Newest",
                SearchText = query.SearchText,
                CategoryId = query.CategoryId,
                BrandId = query.BrandId,
                PriceFrom = query.PriceFrom,
                PriceTo = query.PriceTo,
                Size = query.Size,
                Color = query.Color,
                TagName = query.TagName,
                Tags = dynamicTags,
                Categories = categories.Where(c => c.IsActive).Select(c => new CategoryFilterItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    ProductCount = categoryCounts.TryGetValue(c.Id, out var cnt) ? cnt : 0
                }).ToList(),
                Brands = brands.Where(b => b.IsActive).Select(b => new BrandFilterItem
                {
                    Id = b.Id,
                    Name = b.Name
                }).ToList()
            };

            return View(model);
        }

        // GET: /Shop/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
                return NotFound();

            // 1. Normal query: product details with variants, images, category, brand, and tags
            var product = await _unitOfWork.ProductRepository.GetProductDetailsById(id, cancellationToken);
            if (product == null || !product.IsActive)
                return NotFound();

            // 2. Normal query: related products in the same category (up to 4 products)
            var relatedProducts = await _unitOfWork.ProductRepository.GetRelatedProductsAsync(
                product.CategoryId, product.Id, 4, cancellationToken);

            // 3. Normal query: approved reviews for this product
            var reviews = await _unitOfWork.ProductReviewRepository.GetApprovedReviewsByProductIdAsync(
                id, cancellationToken);

            var reviewInput = new ShopReviewSubmitModel
            {
                ProductId = id
            };

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdStr, out var uid))
                {
                    var customer = await _unitOfWork.CustomerRepository.GetCustomerByUserIdAsync(uid, cancellationToken);
                    if (customer != null)
                    {
                        reviewInput.ReviewerName = $"{customer.FirstName} {customer.LastName}".Trim();
                        reviewInput.ReviewerEmail = customer.Email;
                    }
                }
            }

            var model = new ShopDetailsModel
            {
                Product = product,
                RelatedProducts = relatedProducts.ToList(),
                Reviews = reviews.ToList(),
                ReviewInput = reviewInput
            };

            return View(model);
        }

        // POST: /Shop/AddReview
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(ShopReviewSubmitModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Please fill in all required fields." });
                }
                return RedirectToAction(nameof(Details), new { id = model.ProductId });
            }

            Guid? customerId = null;
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdStr, out var uid))
                {
                    var customer = await _unitOfWork.CustomerRepository.GetCustomerByUserIdAsync(uid, cancellationToken);
                    customerId = customer?.Id;
                }
            }

            var (success, message) = await _mediator.SendCommandAsync<Application.Features.Reviews.Commands.ProductReviewAddCustomerCommand, (bool Success, string Message)>(
                new Application.Features.Reviews.Commands.ProductReviewAddCustomerCommand
                {
                    ProductId = model.ProductId,
                    CustomerId = customerId,
                    ReviewerName = model.ReviewerName,
                    ReviewerEmail = model.ReviewerEmail,
                    Rating = model.Rating,
                    Title = model.Title,
                    Comment = model.Comment
                }, cancellationToken);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success, message });
            }

            TempData["ReviewMessage"] = message;
            return RedirectToAction(nameof(Details), new { id = model.ProductId });
        }
    }
}
