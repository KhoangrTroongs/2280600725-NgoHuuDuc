using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NgoHuuDuc_2280600725.Data;
using NgoHuuDuc_2280600725.Models;
using NgoHuuDuc_2280600725.Responsitories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Cấu hình mật khẩu
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // Cấu hình khóa tài khoản
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30); // Thời gian khóa mặc định
    options.Lockout.MaxFailedAccessAttempts = 5; // Số lần đăng nhập sai tối đa
    options.Lockout.AllowedForNewUsers = true; // Cho phép khóa tài khoản mới
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();

builder.Services.AddControllersWithViews();

// Add this line in service configuration
builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// Trước app.UseStaticFiles();
if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users")))
{
    Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "users"));
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Seed Administrator Role và tài khoản admin
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    // Tạo vai trò Administrator nếu chưa tồn tại
    if (!await roleManager.RoleExistsAsync("Administrator"))
    {
        await roleManager.CreateAsync(new IdentityRole("Administrator"));
    }

    // Tạo các vai trò khác
    var roles = new[] { "User", "Staff", "Manager" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Tạo tài khoản Admin và Admin2
    var adminUsers = new[]
    {
        new { Email = "admin@example.com", Password = "Admin@123" },
        new { Email = "admin2@example.com", Password = "Admin@123" }
    };

    foreach (var adminInfo in adminUsers)
    {
        var adminUser = await userManager.FindByEmailAsync(adminInfo.Email);
        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = adminInfo.Email,
                Email = adminInfo.Email,
                EmailConfirmed = true,
                FullName = "Administrator",
                DateOfBirth = DateTime.Now,
                Address = "Admin Address",
                Gender = Gender.Male
            };

            var result = await userManager.CreateAsync(admin, adminInfo.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Administrator");
            }
        }
    }
}

app.Run();