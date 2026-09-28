using MaleFashion.Application.Contracts;
using MaleFashion.Web.Models.Ordering;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MaleFashion.Web.Controllers.Ordering
{
    [Route("Order")]
    public class OrderTrackingController : Controller
    {
        private readonly IApplicationUnitOfWork _unitOfWork;

        public OrderTrackingController(IApplicationUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("Track")]
        public IActionResult Index(string? orderNumber)
        {
            var model = new OrderTrackingModel();
            if (!string.IsNullOrWhiteSpace(orderNumber))
            {
                model.OrderNumber = orderNumber.Trim();
            }
            return View(model);
        }

        [HttpPost("Track")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(OrderTrackingModel model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(model);

            model.Searched = true;
            var cleanOrderNumber = model.OrderNumber.Trim();
            var cleanEmail = model.Email.Trim().ToLowerInvariant();

            var order = await _unitOfWork.OrderRepository.GetOrderByOrderNumberWithDetailsAsync(cleanOrderNumber, cancellationToken);

            if (order == null || order.Customer == null || !string.Equals(order.Customer.Email, cleanEmail, StringComparison.OrdinalIgnoreCase))
            {
                model.ErrorMessage = "No order was found matching that Order Number and Email address. Please verify your details and try again.";
                return View(model);
            }

            model.Order = order;
            return View(model);
        }
    }
}

