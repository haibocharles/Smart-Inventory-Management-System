using LHP_Inventory_management_system_MVC.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//添加登入驗證服務
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // 登路頁面路径
        options.AccessDeniedPath = "/Account/AccessDenied"; // 访问被拒绝页面
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie有效期
    });




// 添加会话服务 - 修复错误的关键
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 配置仓储服务
var connectionString = "Server=localhost;Database=InventoryManagementSystemDB;Uid=root;Pwd=Aa0925129251;";
// 註冊所有需要的 Repository
builder.Services.AddScoped<PartRepository>(provider => new PartRepository(connectionString));
builder.Services.AddScoped<UserRepository>(provider => new UserRepository(connectionString));
builder.Services.AddScoped<OrderRepository>(provider => new OrderRepository(connectionString)); // 添加這行

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 添加会话中间件 - 必须在 UseRouting 之后，UseAuthorization 之前
app.UseSession();

// 添加身份验证和授权中间件 - 顺序很重要！
app.UseAuthentication(); // 先添加身份验证
app.UseAuthorization();  // 再添加授权

// 配置路由
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();