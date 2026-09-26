using MaleFashion.Web.Models.Faq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaleFashion.Web.Controllers.Faq
{
    [Route("faq")]
    [Route("faqs")]
    public class FaqController : Controller
    {
        [HttpGet]
        public IActionResult Index(string? category, string? search)
        {
            var allItems = GetDefaultFaqItems();
            var categories = new List<FaqCategoryModel>
            {
                new() { Key = "All", Title = "All Questions", IconClass = "fa fa-list-ul", Count = allItems.Count },
                new() { Key = "Ordering", Title = "Orders & Tracking", IconClass = "fa fa-shopping-bag", Count = allItems.Count(x => x.Category == "Ordering") },
                new() { Key = "Shipping", Title = "Shipping & Delivery", IconClass = "fa fa-truck", Count = allItems.Count(x => x.Category == "Shipping") },
                new() { Key = "Payment", Title = "Payment & Pricing", IconClass = "fa fa-credit-card", Count = allItems.Count(x => x.Category == "Payment") },
                new() { Key = "Returns", Title = "Returns & Exchanges", IconClass = "fa fa-refresh", Count = allItems.Count(x => x.Category == "Returns") },
                new() { Key = "Account", Title = "Account & Wishlist", IconClass = "fa fa-user-circle-o", Count = allItems.Count(x => x.Category == "Account") }
            };

            var filteredItems = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(category) && !category.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                filteredItems = filteredItems.Where(x => x.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim();
                filteredItems = filteredItems.Where(x =>
                    x.Question.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    x.Answer.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            var model = new FaqIndexModel
            {
                SelectedCategory = string.IsNullOrWhiteSpace(category) ? "All" : category,
                SearchQuery = search,
                Categories = categories,
                FaqItems = filteredItems.OrderBy(x => x.DisplayOrder).ToList()
            };

            return View(model);
        }

        private static List<FaqItemModel> GetDefaultFaqItems()
        {
            return new List<FaqItemModel>
            {
                // Ordering
                new()
                {
                    Id = 1,
                    Category = "Ordering",
                    Question = "How do I place an order on Male Fashion?",
                    Answer = "Simply browse our Shop catalog, select your preferred product size and color, and click 'Add to Cart'. When you're ready, proceed to the Shopping Cart, review your items, and click Checkout. You can complete your purchase as a guest or with a registered account.",
                    DisplayOrder = 1
                },
                new()
                {
                    Id = 2,
                    Category = "Ordering",
                    Question = "Can I modify or cancel my order after it has been placed?",
                    Answer = "Yes! You can modify or cancel your order before it enters the 'Shipped' status. Please contact our support team immediately via our Contact Us page or hotline with your Order Number.",
                    DisplayOrder = 2
                },
                new()
                {
                    Id = 3,
                    Category = "Ordering",
                    Question = "How do I track my order status?",
                    Answer = "Once your order is confirmed, you will receive an SMS and email notification with your tracking details. Registered users can also view real-time tracking anytime under 'Manage Profile' > 'My Orders'.",
                    DisplayOrder = 3
                },

                // Shipping & Delivery
                new()
                {
                    Id = 4,
                    Category = "Shipping",
                    Question = "What are the delivery timelines across Bangladesh?",
                    Answer = "Inside Dhaka: Delivery typically takes 24 to 48 hours. Outside Dhaka: Delivery across all districts takes 3 to 5 business days via our courier partners.",
                    DisplayOrder = 4
                },
                new()
                {
                    Id = 5,
                    Category = "Shipping",
                    Question = "What are the shipping charges?",
                    Answer = "Standard shipping inside Dhaka is 60 BDT, and outside Dhaka is 120 BDT. We also offer FREE shipping across Bangladesh on qualifying orders exceeding 3,000 BDT or with active promotional coupons.",
                    DisplayOrder = 5
                },
                new()
                {
                    Id = 6,
                    Category = "Shipping",
                    Question = "Can I inspect the parcel upon delivery?",
                    Answer = "Yes! We support open-box inspection in front of the delivery agent for Cash on Delivery orders to ensure complete peace of mind.",
                    DisplayOrder = 6
                },

                // Payment & Pricing
                new()
                {
                    Id = 7,
                    Category = "Payment",
                    Question = "What payment methods are supported?",
                    Answer = "We accept Cash on Delivery (COD), Mobile Financial Services (bKash, Nagad, Rocket), as well as Visa, Mastercard, and American Express. All transactions and prices on our store are processed strictly in Bangladeshi Taka (BDT).",
                    DisplayOrder = 7
                },
                new()
                {
                    Id = 8,
                    Category = "Payment",
                    Question = "How do I apply a discount coupon?",
                    Answer = "On the Shopping Cart page, enter your promotional voucher code in the 'Discount Codes' input box and click 'Apply'. If valid, the discount amount will instantly deduct from your cart total in BDT.",
                    DisplayOrder = 8
                },

                // Returns & Exchanges
                new()
                {
                    Id = 9,
                    Category = "Returns",
                    Question = "What is your return & refund guarantee?",
                    Answer = "We offer a hassle-free 30-day return policy! If an item doesn't fit, is damaged, or differs from the product description, you can request an exchange or full refund within 30 days of receiving your package.",
                    DisplayOrder = 9
                },
                new()
                {
                    Id = 10,
                    Category = "Returns",
                    Question = "What conditions must returned items meet?",
                    Answer = "Items must be unworn, unwashed, with all original tags attached and in their original packaging. Proof of purchase (Order Invoice or confirmation SMS) is required.",
                    DisplayOrder = 10
                },
                new()
                {
                    Id = 11,
                    Category = "Returns",
                    Question = "How long does a refund take to process?",
                    Answer = "Once our quality assurance team receives and verifies your returned item, your refund is disbursed within 3 to 7 business days via your original payment method (bKash/Nagad or bank card).",
                    DisplayOrder = 11
                },

                // Account & Wishlist
                new()
                {
                    Id = 12,
                    Category = "Account",
                    Question = "Can I save items to my Wishlist without signing in?",
                    Answer = "Yes! You can click the heart icon on any product to save it to your guest wishlist. If you later log in or register, your saved items will automatically sync with your permanent account.",
                    DisplayOrder = 12
                },
                new()
                {
                    Id = 13,
                    Category = "Account",
                    Question = "How do I reset my account password if I forgot it?",
                    Answer = "Click 'Sign In' at the top of the page, then click 'Forgot your password?'. Enter your registered email address and we will send you a secure password reset link.",
                    DisplayOrder = 13
                }
            };
        }
    }
}

