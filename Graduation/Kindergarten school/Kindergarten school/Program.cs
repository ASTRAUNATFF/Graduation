using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;
using Kindergarten_school.Models;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình MongoDB
builder.Services.AddSingleton<IMongoClient, MongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("MongoDb");
    return new MongoClient(connectionString);
});
builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    return mongoClient.GetDatabase("db2024"); // Tên database của bạn
});

// Cấu hình MVC
builder.Services.AddControllersWithViews();

// Cấu hình dịch vụ xác thực và phân quyền
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn khi người dùng chưa đăng nhập
        options.LogoutPath = "/Account/Logout"; // Đường dẫn đăng xuất
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian sống của phiên
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware xử lý yêu cầu
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Thêm middleware xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();

// Cấu hình route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
