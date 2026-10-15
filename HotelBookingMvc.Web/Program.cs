
using HotelBookingMvc.Web.Hubs;
using HotelBookingMvc.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hotelbooking.Database.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login";                  // Login page path
        options.AccessDeniedPath = "/Login";           // Access denied path
        options.LogoutPath = "/Login/Logout";          // Logout path
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie expiry time
        options.SlidingExpiration = true;              // Auto-refresh expiry
        options.Cookie.HttpOnly = true;                // Prevent XSS
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Supports both HTTP and HTTPS
        options.Cookie.SameSite = SameSiteMode.Lax;    // Smooth navigation flow
    });

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
        ?? builder.Configuration.GetConnectionString("DbConnection")
        ?? "Server=.;Database=HotelBookingDB;User Id=sa;Password=sasa@123;TrustServerCertificate=True;"));

// SignalR
builder.Services.AddSignalR();

// Services
builder.Services.AddScoped<Customer_Service>();
builder.Services.AddScoped<RoomType_Service>();
builder.Services.AddScoped<Room_Service>();
builder.Services.AddScoped<Service_Service>();
builder.Services.AddScoped<Booking_Service>();
builder.Services.AddScoped<Payment_Service>();
builder.Services.AddScoped<CheckIn_Service>();
builder.Services.AddScoped<CheckOut_Service>();
builder.Services.AddScoped<Dashboard_Service>();
builder.Services.AddScoped<Report_Service>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<Floor_Service>();
builder.Services.AddScoped<RoomStatus_Service>();
builder.Services.AddScoped<RoomPrice_service>();
builder.Services.AddScoped<ExtraBedPrice_Service>();


var app = builder.Build();

// Error handling
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

// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// SignalR Hub
app.MapHub<HotelHub>("/hotelHub");

app.Run();