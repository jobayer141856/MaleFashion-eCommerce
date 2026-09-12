using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Domain.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Application.Contracts
{
    public interface IApplicationUnitOfWork : IUnitOfWork
    {
        // Catalogs
        public ICategoryRepository CategoryRepository { get; }
        public IProductRepository ProductRepository { get; }
        public IBrandRepository BrandRepository { get; }
        // Sales & Shopping
        public IOrderRepository OrderRepository { get; }
        public ICartRepository CartRepository { get; }
        // Customer Management
        public ICustomerRepository CustomerRepository { get; }
        // Blog & Content
        public IBlogPostRepository BlogPostRepository { get; }
        public IBlogCommentRepository BlogCommentRepository { get; }
        // Feedback & Reviews
        public IProductReviewRepository ProductReviewRepository { get; }
        // Promotions & Discounts
        public ICouponRepository CouponRepository { get; }
        public IDealCountdownRepository DealCountdownRepository { get; }
        // Customer Support & Marketing
        public IContactInquiryRepository ContactInquiryRepository { get; }
        public INewsletterSubscriberRepository NewsletterSubscriberRepository { get; }
        public IStoreLocationRepository StoreLocationRepository { get; }
        // CMS & Website Showcase
        public ITeamMemberRepository TeamMemberRepository { get; }
        public IPartnerClientRepository PartnerClientRepository { get; }
        public ITestimonialRepository TestimonialRepository { get; }

    }
}
