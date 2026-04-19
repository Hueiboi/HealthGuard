using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
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
                string currentUsername = User.Identity.Name;
                var profile = await _patientProfileService.GetMyProfileAsync(currentUsername);

                // Trả về file Views/Patient/Index.cshtml kèm dữ liệu
                return View(profile);
            }
            catch (Exception ex)
            {
                // Nếu lỗi (ví dụ không tìm thấy user), đá về trang chủ
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Home");
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