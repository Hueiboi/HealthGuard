using HealthGuard.Models.Dto;
using HealthGuard.Services;
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
                string currentUsername = User.FindFirstValue(ClaimTypes.Name);

                if (string.IsNullOrEmpty(currentUsername))
                {
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

        [HttpPost]
        public async Task<IActionResult> UpdateProfile([FromForm] PatientProfileDto request)
        {
            try
            {
                string currentUsername = User.Identity.Name;

                await _patientProfileService.UpdateProfileAsync(request, currentUsername);

                TempData["Success"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Cập nhật thất bại: " + ex.Message;
                return View("Index", request);
            }
        }

    }
}