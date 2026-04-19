using HealthGuard.Data;
using HealthGuard.Security;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ==============================================================
// GIAI ĐOẠN 1: ĐĂNG KÝ SERVICES (Tất cả builder.Services ở đây)
// ==============================================================
builder.Services.AddControllersWithViews();

// 1. Cấu hình Database
builder.Services.AddDbContext<HealthContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection")),
        mySqlOptions =>
        {
            mySqlOptions.CommandTimeout(120);
            mySqlOptions.EnableRetryOnFailure();
        }));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; // Nếu chưa đăng nhập, tự động đá về trang này
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Giữ đăng nhập 7 ngày
    });

// 2. Đăng ký các Service (Chuyển từ dưới lên trên này)
builder.Services.AddHttpClient();
builder.Services.AddScoped<DiagnosticService>();
builder.Services.AddScoped<IJwtUtils, JwtUtils>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<PatientProfileService>();

// 3. Cấu hình JWT Authentication
var jwtSecret = "DayLaMotChuoiBaoMatCucKyDaiDeLamSecretKeyChoJWT1234567890";
var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.ContainsKey("JWT_TOKEN"))
            {
                context.Token = context.Request.Cookies["JWT_TOKEN"];
            }
            return Task.CompletedTask;
        }
    };
});

// ==============================================================
// GIAI ĐOẠN 2: CHỐT ĐƠN (KHÔNG ĐƯỢC ADD SERVICE SAU DÒNG NÀY NỮA)
// ==============================================================
var app = builder.Build();

// ==============================================================
// GIAI ĐOẠN 3: CẤU HÌNH PIPELINE (Tất cả app.Use... ở đây)
// ==============================================================

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<HealthContext>();
    await DatabaseSeeder.SeedAsync(dbContext);
}

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
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();