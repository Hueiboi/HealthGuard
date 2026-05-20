using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Authorize]
    public class PatientController : Controller
    {
        private readonly PatientProfileService _patientProfileService;

        public PatientController(PatientProfileService patientProfileService)
        {
            _patientProfileService = patientProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy Username với các lớp dự phòng để tránh lỗi "Không tìm thấy tài khoản"
                string currentUsername = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(currentUsername))
                {
                    return RedirectToAction("Login", "Auth");
                }

                var profile = await _patientProfileService.GetMyProfileAsync(currentUsername);
                return View(profile);
            }
            catch (Exception ex)
            {
                // Nếu lỗi do phiên đăng nhập cũ/rác, đẩy ra Logout để làm sạch Session
                if (ex.Message.Contains("Không tìm thấy tài khoản"))
                    return RedirectToAction("Logout", "Auth");

                return Content($"Lỗi tải hồ sơ: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromForm] PatientProfileDto request)
        {
            try
            {
                string currentUsername = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);

                // 1. Cập nhật vào Database (Hàm cũ của ông)
                await _patientProfileService.UpdateProfileAsync(request, currentUsername);

                // 2. 🔥 QUAN TRỌNG: Cập nhật lại Identity để Header đổi ảnh ngay lập tức
                // Lấy lại thông tin hồ sơ vừa lưu để có AvatarUrl mới nhất
                var updatedProfile = await _patientProfileService.GetMyProfileAsync(currentUsername);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, currentUsername),
            new Claim("AvatarUrl", updatedProfile.AvatarUrl ?? "") // Nhét cái ảnh vào đây
        };

                var claimsIdentity = new ClaimsIdentity(claims, Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme);

                // Ghi đè Cookie đăng nhập hiện tại
                await HttpContext.SignInAsync(
                    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity)
                );

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Cập nhật thất bại: " + ex.Message;
                return View("Index", request);
            }
        }
    }
}