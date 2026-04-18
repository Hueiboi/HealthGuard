using HealthGuard.Data;
using HealthGuard.Services; // Thêm namespace chứa AuthService
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Giữ nguyên MVC hiện có của bạn
builder.Services.AddControllersWithViews();

// 2. Đăng ký Razor Pages và chỉ định gốc nằm trong Views/Pages
builder.Services.AddRazorPages()
    .WithRazorPagesRoot("/Views/Pages");

// 3. Đăng ký AuthService (Rất quan trọng để RegisterModel có thể gọi được nó)
builder.Services.AddScoped<AuthService>();

// 4. Giữ nguyên cấu hình DbContext của bạn
builder.Services.AddDbContext<HealthContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // .NET 8 chuẩn

app.UseRouting();

app.UseAuthorization();

// 5. Ánh xạ Razor Pages (Để hệ thống nhận diện các file .cshtml trong Views/Pages)
app.MapRazorPages();

// 6. Giữ nguyên Route mặc định của MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();