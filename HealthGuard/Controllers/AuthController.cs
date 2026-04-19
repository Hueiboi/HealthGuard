using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    // ĐÃ XOÁ: [ApiController] và [Route] để dùng được return View()
    public class AuthController : Controller 
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // ==========================================
        // 1. TRẢ VỀ GIAO DIỆN (HTML/Razor View)
        // ==========================================

        [HttpGet("Auth/Login")]
        public IActionResult Login()
        {
            return View(); // Lúc này C# sẽ tìm file Views/Auth/Login.cshtml
        }

        [HttpGet("Auth/Register")]
        public IActionResult Register()
        {
            return View(); // Lúc này C# sẽ tìm file Views/Auth/Register.cshtml
        }

        // ==========================================
        // 2. XỬ LÝ LOGIC (API/Postback)
        // ==========================================

        // Xử lý Đăng ký
        [HttpPost("Auth/Register")]
        public async Task<IActionResult> RegisterUserAsync([FromForm] RegisterRequestDto request)
        {
            // Lưu ý: Đổi từ [FromBody] sang [FromForm] nếu ông dùng thẻ <form> truyền thống
            try
            {
                var registeredUser = await _authService.RegisterAsync(request);
                return RedirectToAction("Login"); // Đăng ký xong cho sang trang Login luôn
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Đăng ký thất bại: " + ex.Message;
                return View("Register");
            }
        }

        // Xử lý Đăng nhập
        [HttpPost("Auth/Login")]
        public async Task<IActionResult> LoginUserAsync([FromForm] LoginRequestDto request)
        {
            try
            {
                var token = await _authService.LoginAsync(request);

                // Nếu ông dùng JWT Token, có thể lưu vào Cookie ở đây để các trang sau dùng
                // Tạm thời mình cứ cho nó vào trang chủ đã
                return RedirectToAction("Index", "Home");
            }
            catch (Exception)
            {
                ViewBag.Error = "Tên đăng nhập hoặc mật khẩu không đúng!";
                return View("Login");
            }
        }
    }
}