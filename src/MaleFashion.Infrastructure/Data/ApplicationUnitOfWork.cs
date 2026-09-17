using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Repositories.Blogs;
using MaleFashion.Application.Contracts.Repositories.Carts;
using MaleFashion.Application.Contracts.Repositories.Catalogs;
using MaleFashion.Application.Contracts.Repositories.CMS;
using MaleFashion.Application.Contracts.Repositories.Customers;
using MaleFashion.Application.Contracts.Repositories.Ordering;
using MaleFashion.Application.Contracts.Repositories.Promotions;
using MaleFashion.Application.Contracts.Repositories.Reviews;
using MaleFashion.Application.Contracts.Repositories.Support;
using MaleFashion.Application.Contracts.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Data
{
    public class ApplicationUnitOfWork : UnitOfWork, IApplicationUnitOfWork
    {
        // Catalogs
        public ICategoryRepository CategoryRepository { get; private set; }
        public IProductRepository ProductRepository { get; private set; }
        public IBrandRepository BrandRepository { get; private set; }
        // Sales & Shopping
        public IOrderRepository OrderRepository { get; private set; }
        public ICartRepository CartRepository { get; private set; }
        // Customer Management
        public ICustomerRepository CustomerRepository { get; private set; }
        // Blog & Content
        public IBlogPostRepository BlogPostRepository { get; private set; }
        public IBlogCommentRepository BlogCommentRepository { get; private set; }
        // Feedback & Reviews
        public IProductReviewRepository ProductReviewRepository { get; private set; }
        // Promotions & Discounts
        public ICouponRepository CouponRepository { get; private set; }
        public IDealCountdownRepository DealCountdownRepository { get; private set; }
        // Customer Support & Marketing
        public IContactInquiryRepository ContactInquiryRepository { get; private set; }
        public INewsletterSubscriberRepository NewsletterSubscriberRepository { get; private set; }
        public IStoreLocationRepository StoreLocationRepository { get; private set; }
        // CMS & Website Showcase
        public ITeamMemberRepository TeamMemberRepository { get; private set; }
        public IPartnerClientRepository PartnerClientRepository { get; private set; }
        public ITestimonialRepository TestimonialRepository { get; private set; }
        public IHeroSlideRepository HeroSlideRepository { get; private set; }

        public ISqlUtility SqlUtility { get; private set; }

        public ApplicationUnitOfWork(
            ApplicationDbContext dbContext,
            ICategoryRepository categoryRepository,
            IProductRepository productRepository,
            IBrandRepository brandRepository,
            IOrderRepository orderRepository,
            ICartRepository cartRepository,
            ICustomerRepository customerRepository,
            IBlogPostRepository blogPostRepository,
            IProductReviewRepository productReviewRepository,
            ICouponRepository couponRepository,
            IDealCountdownRepository dealCountdownRepository,
            IContactInquiryRepository contactInquiryRepository,
            INewsletterSubscriberRepository newsletterSubscriberRepository,
            IStoreLocationRepository storeLocationRepository,
            ITeamMemberRepository teamMemberRepository,
            IPartnerClientRepository partnerClientRepository,
            ITestimonialRepository testimonialRepository,
            IHeroSlideRepository heroSlideRepository) : base(dbContext)
        {
            CategoryRepository = categoryRepository;
            ProductRepository = productRepository;
            BrandRepository = brandRepository;
            OrderRepository = orderRepository;
            CartRepository = cartRepository;
            CustomerRepository = customerRepository;
            BlogPostRepository = blogPostRepository;
            ProductReviewRepository = productReviewRepository;
            CouponRepository = couponRepository;
            DealCountdownRepository = dealCountdownRepository;
            ContactInquiryRepository = contactInquiryRepository;
            NewsletterSubscriberRepository = newsletterSubscriberRepository;
            StoreLocationRepository = storeLocationRepository;
            TeamMemberRepository = teamMemberRepository;
            PartnerClientRepository = partnerClientRepository;
            TestimonialRepository = testimonialRepository;
            HeroSlideRepository = heroSlideRepository;
            SqlUtility = new SqlUtility(dbContext.Database.GetDbConnection());
        }
    }
}
