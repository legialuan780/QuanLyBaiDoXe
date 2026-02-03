using Microsoft.EntityFrameworkCore;
using QuanLyBaiDoXe.Models.EF;
using QuanLyBaiDoXe.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;

// Set console encoding to UTF-8 for Vietnamese characters
Console.OutputEncoding = Encoding.UTF8;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Ensure JSON responses use UTF-8 encoding for Vietnamese characters
        options.JsonSerializerOptions.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
    });

//add connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<QuanLyBaiDoXeContext>(options => options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

// Register services
builder.Services.AddScoped<IVehicleEntryService, VehicleEntryService>();
builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Register License Plate Recognition service
builder.Services.AddHttpClient<ILicensePlateRecognitionService, PlateRecognizerService>();

// Register MoMo Payment service
builder.Services.AddHttpClient<IMoMoService, MoMoService>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    });

// Add Session (optional, for additional state management)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Redirect root URL to Login
app.MapGet("/", context =>
{
    context.Response.Redirect("/Account/Login");
    return Task.CompletedTask;
});

// Route cho Area Admin
app.MapControllerRoute(
    name: "admin",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}",
    constraints: new { area = "Admin" });

// Route cho Area User (Khách hàng)
app.MapControllerRoute(
    name: "user",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}",
    constraints: new { area = "User" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
