using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using CourtCaseTrackingSystem.Data;
using CourtCaseTrackingSystem.Models;
using QuestPDF.Infrastructure; // Added for PDF generation

var builder = WebApplication.CreateBuilder(args);

// Configure QuestPDF license (must come first)
QuestPDF.Settings.License = LicenseType.Community;

// 1. Database Configuration
builder.Services.AddDbContext<CourtDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
})
.AddEntityFrameworkStores<CourtDbContext>()
.AddDefaultUI() // For identity pages
.AddDefaultTokenProviders();

// 3. Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("JudgeOnly", policy => policy.RequireRole("Judge"));
    options.AddPolicy("RegistrarOnly", policy => policy.RequireRole("Registrar"));
});

// 4. Security Configuration
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-CSRF-TOKEN";
    options.SuppressXFrameOptionsHeader = false;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DefenseUpload", policy => 
        policy.RequireRole("Judge", "Clerk"));
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WitnessUpload", policy => 
        policy.RequireRole("Judge", "Clerk"));
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("NewUpload", policy => 
        policy.RequireRole("Registrar"));
});
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50MB
});


// 5. MVC Services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddSingleton<ISmsService, MockSmsService>();
// 6. Application Services
builder.Services.AddScoped<JudgeAssignmentService>();
builder.Services.AddScoped<FileStorageService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// 7. Database Initialization
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        // This single method handles both roles and admin user
        await DbInitializer.SeedAllAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// 8. Middleware Pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();