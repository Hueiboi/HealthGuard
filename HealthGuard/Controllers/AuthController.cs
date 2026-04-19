using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    public class AuthController : Controller
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // ==========================================
        // 1. TRANG ĐĂNG NHẬP (Lấy giao diện & Xử lý)
        // ==========================================

        // Mở giao diện Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Nhận dữ liệu khi bấm nút Đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login([FromForm] LoginRequestDto request)
        {
            try
            {
                // 1. Service vẫn chạy và check mật khẩu, trả về JWT (mặc dù MVC mình không dùng trực tiếp JWT này để Auth)
                var token = await _authService.LoginAsync(request);

                // 2. TẠO THẺ COOKIE CHO MVC HIỂU
                // (Thực tế ông có thể lấy Username/Email thẳng từ request để làm Claims)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, request.Username), // Lấy tên hiển thị ra màn hình
                    new Claim("jwt_token", token) // Cất cái token này vào Cookie, sau này thích thì lôi ra xài
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. ĐÓNG DẤU ĐĂNG NHẬP (Lưu Cookie xuống trình duyệt)
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                // 4. Vào trang chủ thôi!
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message; // Lấy đúng câu chửi "Sai tài khoản, mật khẩu..." từ Service hiện ra
                return View();
            }
        }

        // ==========================================
        // 2. TRANG ĐĂNG KÝ (Lấy giao diện & Xử lý)
        // ==========================================

        // Mở giao diện Đăng ký
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // Nhận dữ liệu khi bấm nút Đăng ký
        [HttpPost]
        public async Task<IActionResult> Register([FromForm] RegisterRequestDto request)
        {
            // 1. Kiểm tra xem form có điền thiếu gì không
            if (!ModelState.IsValid)
            {
                var errors = string.Join(" | ", ModelState.Values
                                    .SelectMany(v => v.Errors)
                                    .Select(e => e.ErrorMessage));
                ViewBag.Error = "Lỗi nhập liệu: " + errors;
                return View(request);
            }

            try
            {
                // 2. Gọi service để lưu vào DB
                await _authService.RegisterAsync(request);

                // 3. Chuyển hướng sang trang Login sau khi thành công
                // Nó sẽ tìm đến hàm [HttpGet] Login trong cùng Controller này
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                // Móc cái lỗi thật sự (Inner Exception) ra
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                // Quăng nó ra màn hình
                ViewBag.Error = "Lỗi Database: " + realError;
                return View(request);
            }
        }
    }
}