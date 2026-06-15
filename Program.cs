using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PharmaTrackPro.Data;
using PharmaTrackPro.Interfaces;
using PharmaTrackPro.Models.Identity;
using PharmaTrackPro.Repositories;
using PharmaTrackPro.Services;
using QuestPDF.Infrastructure;
using Serilog;

// ---------------------------------------------------------------------------
// Bootstrap Serilog early so startup failures are logged too.
// ---------------------------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Serilog (reads full config from appsettings.json "Serilog" section)
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((context, services, configuration) =>
        configuration.ReadFrom.Configuration(context.Configuration));

    // -----------------------------------------------------------------------
    // QuestPDF license (Community license — free for this use case)
    // -----------------------------------------------------------------------
    QuestPDF.Settings.License = LicenseType.Community;

    // -----------------------------------------------------------------------
    // Database (SQL Server Express, Code First)
    // -----------------------------------------------------------------------
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    // -----------------------------------------------------------------------
    // ASP.NET Core Identity
    // -----------------------------------------------------------------------
    builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password policy — reasonable defaults for a pharmacy back office
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;

            // Lockout policy
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;

            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

    // -----------------------------------------------------------------------
    // MVC
    // -----------------------------------------------------------------------
    builder.Services.AddControllersWithViews();
    builder.Services.AddHttpContextAccessor();

    // -----------------------------------------------------------------------
    // Repositories & Services (registered per module as each is built)
    // -----------------------------------------------------------------------
    builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
    builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
    builder.Services.AddScoped<IManufacturerService, ManufacturerService>();
    builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
    builder.Services.AddScoped<IMedicineService, MedicineService>();
    builder.Services.AddScoped<IBarcodeGeneratorService, BarcodeGeneratorService>();
    builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
    builder.Services.AddScoped<ISupplierService, SupplierService>();
    builder.Services.AddScoped<IPurchaseRepository, PurchaseRepository>();
    builder.Services.AddScoped<IPurchaseService, PurchaseService>();
    builder.Services.AddScoped<IBatchRepository, BatchRepository>();
    builder.Services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
    builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
    builder.Services.AddScoped<IInventoryService, InventoryService>();
    builder.Services.AddScoped<IExpiryService, ExpiryService>();
    builder.Services.AddScoped<IDashboardService, DashboardService>();
    builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
    builder.Services.AddScoped<ICustomerService, CustomerService>();
    builder.Services.AddScoped<ISalesService, SalesService>();
    builder.Services.AddScoped<ISaleRepository, SaleRepository>();
    builder.Services.AddScoped<ISaleReturnRepository, SaleReturnRepository>();
    builder.Services.AddScoped<ISaleReturnService, SaleReturnService>();
    builder.Services.AddScoped<IReportService, ReportService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IAuditLogService, AuditLogService>();
    builder.Services.AddScoped<IUserManagementService, UserManagementService>();
    builder.Services.AddScoped<ISettingsService, SettingsService>();

    var app = builder.Build();

    // -----------------------------------------------------------------------
    // Seed default roles + administrator on startup
    // -----------------------------------------------------------------------
    using (var scope = app.Services.CreateScope())
    {
        await SeedData.InitializeAsync(scope.ServiceProvider, app.Configuration);
    }

    // -----------------------------------------------------------------------
    // HTTP pipeline
    // -----------------------------------------------------------------------
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseSerilogRequestLogging();

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Account}/{action=Login}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "PharmaTrack Pro terminated unexpectedly during startup");
}
finally
{
    Log.CloseAndFlush();
}
