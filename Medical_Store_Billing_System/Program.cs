using Medical_Store_Billing_System.Models;

using MedicalStore.Business.Interfaces;
using MedicalStore.Business.Mappings;
using MedicalStore.Business.Services;

using MedicalStore.Data;

using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Business.Services;

using MedicalStore.MedicalStore.Data.Speed;

using MedicalStore.MedicalStore.Repository.Implementations;
using MedicalStore.MedicalStore.Repository.Interface;

using MedicalStore.Repository.Implementations;
using MedicalStore.Repository.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// MVC
// ============================================================

builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();

// ============================================================
// DATABASE
// ============================================================

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// ============================================================
// IDENTITY
// ============================================================

builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// ============================================================
// COOKIE CONFIGURATION
// ============================================================

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
});

// ============================================================
// SESSION
// ============================================================

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);

    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ============================================================
// AUTOMAPPER - AUTO MAPPER 16
// ============================================================
//
// AutoMapper 16 no longer uses:
// AddAutoMapper(typeof(MappingProfile))
//
// Instead configure AutoMapper through the configuration
// delegate.
//
// ============================================================

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<MappingProfile>();
});

// ============================================================
// UNIT OF WORK
// ============================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ============================================================
// REPOSITORIES
// ============================================================

builder.Services.AddScoped<
    IMedicineCategoryRepository,
    MedicineCategoryRepository>();

builder.Services.AddScoped<
    IBrandRepository,
    BrandRepository>();

builder.Services.AddScoped<
    IMedicineMasterRepository,
    MedicineMasterRepository>();

builder.Services.AddScoped<
    ICustomerRepository,
    CustomerRepository>();

builder.Services.AddScoped<
    ISupplierRepository,
    SupplierRepository>();

builder.Services.AddScoped<
    ISaleRepository,
    SaleRepository>();

builder.Services.AddScoped<
    ISaleDetailsRepository,
    SaleDetailsRepository>();

builder.Services.AddScoped<
    IPurchaseRepository,
    PurchaseRepository>();

builder.Services.AddScoped<
    IPurchaseDetailsRepository,
    PurchaseDetailsRepository>();

builder.Services.AddScoped<
    IReceiptRepository,
    ReceiptRepository>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

// ============================================================
// BUSINESS SERVICES
// ============================================================

builder.Services.AddScoped<
    IMedicineCategoryService,
    MedicineCategoryService>();

builder.Services.AddScoped<
    IBrandService,
    BrandService>();

builder.Services.AddScoped<
    IMedicineMasterService,
    MedicineMasterService>();

builder.Services.AddScoped<
    ICustomerService,
    CustomerService>();

builder.Services.AddScoped<
    ISupplierService,
    SupplierService>();

builder.Services.AddScoped<
    ISaleService,
    SaleService>();

builder.Services.AddScoped<
    IPurchaseService,
    PurchaseService>();

builder.Services.AddScoped<
    IReceiptService,
    ReceiptService>();

builder.Services.AddScoped<
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.AddScoped<
    IReportService,
    ReportService>();

// ============================================================
// BUILD APPLICATION
// ============================================================

var app = builder.Build();

// ============================================================
// HTTP PIPELINE
// ============================================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

// HTTPS
app.UseHttpsRedirection();

// Static files
app.UseStaticFiles();

// Routing
app.UseRouting();

// Session
app.UseSession();

// Authentication
app.UseAuthentication();

// Authorization
app.UseAuthorization();

// ============================================================
// DATABASE SEEDING
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        await DbInitializer.SeedAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services
            .GetRequiredService<ILogger<Program>>();

        logger.LogError(
            ex,
            "An error occurred while seeding the database.");
    }
}

// ============================================================
// DEFAULT ROUTE
// ============================================================

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// ============================================================
// RUN
// ============================================================

app.Run();