using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MaleFashion.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;


namespace MaleFashion.Infrastructure.Extensions
{
    public static class DbContexExtension
    {
        public static void AddDbContext(this IServiceCollection services, string connectionString, Assembly migrationAssembly)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString,
                x => x.MigrationsAssembly(migrationAssembly)));
        }
    }
}
