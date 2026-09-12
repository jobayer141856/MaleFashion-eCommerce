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
using MaleFashion.Infrastructure.Data;
using MaleFashion.Infrastructure.Data.Repositories.Blogs;
using MaleFashion.Infrastructure.Data.Repositories.Carts;
using MaleFashion.Infrastructure.Data.Repositories.Catalogs;
using MaleFashion.Infrastructure.Data.Repositories.CMS;
using MaleFashion.Infrastructure.Data.Repositories.Customers;
using MaleFashion.Infrastructure.Data.Repositories.Ordering;
using MaleFashion.Infrastructure.Data.Repositories.Promotions;
using MaleFashion.Infrastructure.Data.Repositories.Reviews;
using MaleFashion.Infrastructure.Data.Repositories.Support;
using MaleFashion.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace MaleFashion.Infrastructure.Extensions
{
    public static class DependencyInjections
    {
        public static IServiceCollection AddInfrastructureDependency(this IServiceCollection services, string rootPath)
        {
            services.AddScoped<IApplicationUnitOfWork, ApplicationUnitOfWork>();
            services.AddSingleton<IFileStorageService, FileStorageService>(x => new FileStorageService(rootPath));
            services.AddScoped<IEmailService, EmailService>();

            // =========================================================
            // REPOSITORIES
            // =========================================================
            // Catalogs
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IBrandRepository, BrandRepository>();
            // Sales & Shopping
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            // Customers
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            // Blogs
            services.AddScoped<IBlogPostRepository, BlogPostRepository>();
            // Reviews & Feedback
            services.AddScoped<IProductReviewRepository, ProductReviewRepository>();
            // Promotions & Deals
            services.AddScoped<ICouponRepository, CouponRepository>();
            services.AddScoped<IDealCountdownRepository, DealCountdownRepository>();
            // Customer Support & Inquiries
            services.AddScoped<IContactInquiryRepository, ContactInquiryRepository>();
            services.AddScoped<INewsletterSubscriberRepository, NewsletterSubscriberRepository>();
            services.AddScoped<IStoreLocationRepository, StoreLocationRepository>();
            // Website CMS
            services.AddScoped<ITeamMemberRepository, TeamMemberRepository>();
            services.AddScoped<IPartnerClientRepository, PartnerClientRepository>();
            services.AddScoped<ITestimonialRepository, TestimonialRepository>();
            return services;
        }

    }
}
