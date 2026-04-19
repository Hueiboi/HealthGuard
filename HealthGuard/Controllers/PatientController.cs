using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Authorize] // Bắt buộc đăng nhập mới được vào
    public class PatientController : Controller // ĐÃ ĐỔI: Thành Controller để trả về View
    {
        private readonly PatientProfileService _patientProfileService;

        public PatientController(PatientProfileService patientProfileService)
        {
            _patientProfileService = patientProfileService;
        }

        // 1. MỞ TRANG GIAO DIỆN HỒ SƠ
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                string currentUsername = User.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(currentUsername))
                {
                    // Nếu vẫn null thì có thể là do chưa Login hoặc Cookie hết hạn
                    return RedirectToAction("Login", "Auth");
                }

                var profile = await _patientProfileService.GetMyProfileAsync(currentUsername);
                return View(profile);
            }
            catch (Exception ex)
            {
                return Content($"Lỗi rồi Nam ơi: {ex.Message} --- Chi tiết: {ex.InnerException?.Message}");
            }
        }

        // 2. NHẬN DỮ LIỆU TỪ NÚT "LƯU THAY ĐỔI"
        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromForm] PatientProfileDto request)
        {
            try
            {
                string currentUsername = User.Identity.Name;

                // Cập nhật xuống DB
                await _patientProfileService.UpdateProfileAsync(request, currentUsername);

                // Hiện thông báo xanh lá cây và load lại trang
                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Nếu lỗi, giữ nguyên trang và báo chữ đỏ
                ViewBag.Error = "Cập nhật thất bại: " + ex.Message;
                return View("Index", request);
            }
        }
    }
}