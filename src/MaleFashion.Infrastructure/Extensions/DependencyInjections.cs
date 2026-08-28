using MaleFashion.Application.Contracts;
using MaleFashion.Application.Contracts.Services;
using MaleFashion.Infrastructure.Data;
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
            // Add your infrastructure services here
            // For example, if you have a repository or service layer, you can register them here
            // services.AddScoped<IYourRepository, YourRepository>();
            // services.AddScoped<IYourService, YourService>();
            // You can also add other infrastructure-related services like logging, caching, etc.
            return services;
        }

    }
}
