using Cortex.Mediator.DependencyInjection;
using MaleFashion.Infrastructure.Data;
using MaleFashion.Infrastructure.Extensions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

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
    #endregion


    builder.Services.AddControllersWithViews();

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
        new[] { typeof(Program) },
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

    app.UseAuthorization();

    app.MapStaticAssets();

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
