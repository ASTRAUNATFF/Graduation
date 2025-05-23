﻿using Kindergarten_school.Controllers;
using Kindergarten_school.Extensions;
using Kindergarten_school.Hubs;
using Kindergarten_school.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
// Cấu hình Authentication
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login"; // Trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Trang từ chối truy cập
    });

builder.Services.AddAuthorization();


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
builder.Services.AddTransient<TransactionsController>();

// Đăng ký ScheduleService
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();


// Cấu hình MVC
builder.Services.AddSignalR();

// Cấu hình dịch vụ xác thực và phân quyền
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn khi người dùng chưa đăng nhập
        options.LogoutPath = "/Account/Logout"; // Đường dẫn đăng xuất
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Thời gian sống của phiên
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("TeacherOnly", policy => policy.RequireRole("Teacher"));
});

// cấu hình session
builder.Services.AddDistributedMemoryCache(); // Bộ nhớ cache phân phối
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Thời gian sống của session
    options.Cookie.HttpOnly = true; // Tăng bảo mật bằng cách chỉ cho phép HTTP truy cập cookie
    options.Cookie.IsEssential = true; // Đảm bảo cookie được gửi trong mọi trường hợp
});

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
app.UseSession();

// Thêm middleware xác thực và phân quyền
app.UseAuthentication();
app.UseAuthorization();



// Cấu hình route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapHub<NotificationHub>("/notificationHub"); // Thêm endpoint SignalR

app.MapHub<ReportsHub>("/reports-hub"); // Thêm endpoint SignalR
app.MapHub<SchoolHub>("/school-hub"); // Thêm endpoint SignalR
app.MapHub<HealthReportHub>("/health-hub"); // Thêm endpoint SignalR
app.MapHub<ChatHub>("/chat-hub"); // Thêm endpoint SignalR
app.Run();
