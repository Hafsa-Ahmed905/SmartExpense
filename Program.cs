using FP.Data;
using FP.Repositories;
using FP.Hubs;
using FP.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrEmpty(databaseUrl)}");

string connectionString;

if (!string.IsNullOrEmpty(databaseUrl))
{
    try
    {
        // Parse Railway's postgres:// URL
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':');
        
        connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};Ssl Mode=Require;Trust Server Certificate=true";
        
        Console.WriteLine($"Converted connection string (password hidden)");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
        throw;
    }
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DATABASE_URL not found and no DefaultConnection configured.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ----------------------
// Identity setup
// ----------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie paths
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LogoutPath = "/Account/Logout";
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// ----------------------
// SignalR, MVC, Session
// ----------------------
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ----------------------
// Repositories & services
// ----------------------
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<ISettingRepository, SettingRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();

var app = builder.Build();

// ----------------------
// Initialize database (creates DB automatically)
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    await DbInitializer.InitializeAsync(services, logger);
}

// ----------------------
// Middleware pipeline
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ----------------------
// Routes
// ----------------------
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "dashboard",
    pattern: "{controller=DashBoard}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/notificationHub");
app.MapRazorPages();

// ----------------------
// FIX FOR RAILWAY: listen on dynamic port
// ----------------------
var port = Environment.GetEnvironmentVariable("PORT") ?? "80";
app.Urls.Clear();
app.Urls.Add($"http://*:{port}");

// ----------------------
// Run the app
// ----------------------
app.Run();


