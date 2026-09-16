using Hotelbooking.Database.Context;
using HotelBookingMvc.Web.Hubs;
using HotelBookingMvc.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// MVC
builder.Services.AddControllersWithViews();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.UseAuthorization();

// MVC route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

// SignalR Hub
app.MapHub<HotelHub>("/hotelHub");

app.Run();