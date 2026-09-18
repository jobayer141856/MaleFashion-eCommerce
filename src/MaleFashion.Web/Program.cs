using Cortex.Mediator.DependencyInjection;
using MaleFashion.Application.Features.Catalogs.Categories.Commands;
using MaleFashion.Domain.Utilities;
using MaleFashion.Infrastructure.Data;
using MaleFashion.Infrastructure.Extensions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Reflection;
using MaleFashion.Web.Areas.Admin.Conventions;
using MaleFashion.Infrastructure.Services.SSLCommerz;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

Log.Logger = new LoggerConfiguration()
      .WriteTo.File(
        "Logs/log-.log",
        rollingInterval: RollingInterval.Day)
      .WriteTo.MSSqlServer(
        connectionString: builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found."),
        sinkOptions: new MSSqlServerSinkOptions { TableName = "Logs", AutoCreateSqlTable = true })
      .CreateBootstrapLogger();

try
{
    //var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    var migrationAssembly = Assembly.GetAssembly(typeof(ApplicationDbContext));


    #region Identity Configuration
    builder.Services.AddIdentity();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";

        options.Events.OnRedirectToLogin = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Admin/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = context =>
        {
            if (context.Request.Path.StartsWithSegments("/Admin", StringComparison.OrdinalIgnoreCase))
            {
                var returnUrl = context.Request.Path + context.Request.QueryString;
                context.Response.Redirect($"/Admin/Account/AccessDenied?returnUrl={Uri.EscapeDataString(returnUrl)}");
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };
    });
    #endregion


    builder.Services.AddControllersWithViews(options =>
    {
        options.Conventions.Add(new AdminAreaAuthorizationConvention());
    });

    #region Serilog Configuration
    builder.Host.UseSerilog((context, lc) =>
    {
        lc.MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(context.Configuration);
    });
    #endregion

    #region Dependency Injection Configuration

    builder.Services.AddInfrastructureDependency(builder.Environment.WebRootPath);
    #endregion

    #region DbContext Configuration

    builder.Services.AddDbContext(connectionString, migrationAssembly);

    #endregion

    #region Cortex Mediator Configuration

    builder.Services.AddCortexMediator(
        new[] { typeof(Program) , typeof(CategoryAddCommand) },
        options => options.AddDefaultBehaviors()

    );
    #endregion

    #region Mapstar Configuration

    //Custom Configuration
    //var config = TypeAdapterConfig.GlobalSettings;
    //config.Scan(typeof(MapsterConfiguration).Assembly);
    //builder.Services.AddSingleton(config);
    //builder.Services.AddScoped<IMapper, ServiceMapper>();

    // Default Configuration
    builder.Services.AddMapster();

    #endregion

    #region AppSettings Configuration
    builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
    builder.Services.Configure<SSLCommerzSettings>(builder.Configuration.GetSection("SSLCommerz"));
    #endregion

    builder.Services.AddRazorPages();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapStaticAssets();
    app.UseStaticFiles();

    app.MapControllerRoute(
        name: "area",
        pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    app.MapRazorPages()
       .WithStaticAssets();

    Log.Information("Application Starting Up");

    app.Run();
}
catch(Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}
