using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class MobileController : ControllerBase
    {
        private readonly PatientProfileService _patientProfileService;
        private readonly MobileService _mobileService;
        private readonly SymptomService _symptomService;

        public MobileController(
            PatientProfileService patientProfileService,
            MobileService mobileService,
            SymptomService symptomService)
        {
            _patientProfileService = patientProfileService;
            _mobileService = mobileService;
            _symptomService = symptomService;
        }

        // ================== CÁC API CẦN TOKEN (ĐÃ ĐĂNG NHẬP) ==================

        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username)) return Unauthorized(new { success = false, message = "Token không hợp lệ" });

                var dashboardData = await _mobileService.GetDashboardDataAsync(username);
                return Ok(new { success = true, bmi = dashboardData.Bmi, bmiStatus = dashboardData.BmiStatus, topDiseaseName = dashboardData.TopDiseaseName, diagnosisScore = dashboardData.DiagnosisScore });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpGet("Profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                var profile = await _patientProfileService.GetMyProfileAsync(username);
                return Ok(new { success = true, data = profile });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpPost("UpdateProfile")]
        public async Task<IActionResult> UpdateProfile([FromBody] PatientProfileDto request)
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                await _patientProfileService.UpdateProfileAsync(request, username);
                return Ok(new { success = true, message = "Cập nhật hồ sơ thành công!" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpGet("Symptoms")]
        public async Task<IActionResult> GetSymptoms()
        {
            try
            {
                var symptoms = await _symptomService.GetAllSymptomsAsync(1, 100, null);
                return Ok(new { success = true, data = symptoms });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpPost("Diagnose")]
        public async Task<IActionResult> RunMobileDiagnosis([FromBody] MobileDiagnosticRequest request)
        {
            if (request == null || request.SymptomIds == null || request.SymptomIds.Count == 0)
                return BadRequest(new { success = false, message = "Vui lòng cung cấp ít nhất một triệu chứng." });

            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                if (string.IsNullOrEmpty(username)) return Unauthorized(new { success = false, message = "Token không hợp lệ" });

                var diagnoses = await _mobileService.ProcessDiagnosisAsync(username, request);
                return Ok(new { success = true, data = diagnoses });
            }
            catch (Exception ex) {
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine("\n[LỖI DATABASE] " + realError + "\n"); // In ra màn hình console C#

                return StatusCode(500, new { success = false, message = "Lỗi Database: " + realError });
            }
        }

        [HttpGet("History")]
        public async Task<IActionResult> GetHistory()
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                var data = await _mobileService.GetDiagnosisHistoryAsync(username);
                return Ok(new { success = true, data = data });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpGet("DiagnosisDetail/{id}")]
        public async Task<IActionResult> GetDiagnosisDetail(long id)
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                var data = await _mobileService.GetDiagnosisDetailAsync(id, username);
                return Ok(new { success = true, data = data });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpDelete("History/{id}")]
        public async Task<IActionResult> DeleteHistory(long id)
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                await _mobileService.DeleteDiagnosisAsync(id, username);
                return Ok(new { success = true, message = "Đã xóa lịch sử chẩn đoán." });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpDelete("History")]
        public async Task<IActionResult> DeleteAllHistory()
        {
            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                await _mobileService.DeleteAllDiagnosisHistoryAsync(username);
                return Ok(new { success = true, message = "Đã xóa toàn bộ lịch sử." });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        // Thêm vào trong class MobileController
        [HttpPost("Feedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Comments))
                return BadRequest(new { success = false, message = "Nội dung góp ý không được để trống." });

            try
            {
                string username = User.FindFirstValue(ClaimTypes.Name);
                await _mobileService.SubmitMobileFeedbackAsync(username, request.SessionId, request.Comments);
                return Ok(new { success = true, message = "Cảm ơn bạn đã đóng góp ý kiến!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // ================== CÁC API KHÔNG CẦN TOKEN (CHO MÀN HÌNH ĐĂNG NHẬP/ĐĂNG KÝ) ==================

        [HttpPost("SendOtp")]
        [AllowAnonymous]
        public async Task<IActionResult> SendOtp([FromBody] MobileSendOtpRequest request)
        {
            if (string.IsNullOrEmpty(request?.PhoneNumber)) return BadRequest(new { success = false, message = "Số điện thoại không hợp lệ" });
            try
            {
                await _mobileService.SendOtpAsync(request.PhoneNumber);
                return Ok(new { success = true, message = "Mã OTP đã được gửi" });
            }
            catch (Exception ex) { return BadRequest(new { success = false, message = ex.Message }); }
        }

        [HttpPost("VerifyOtp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] MobileVerifyOtpRequest request)
        {
            if (string.IsNullOrEmpty(request?.PhoneNumber) || string.IsNullOrEmpty(request?.OtpCode))
                return BadRequest(new { success = false, message = "Dữ liệu không hợp lệ" });

            try
            {
                var result = await _mobileService.VerifyOtpAsync(request.PhoneNumber, request.OtpCode);
                return Ok(new { success = true, message = "Xác thực thành công", token = result.Token, fullName = result.FullName });
            }
            catch (UnauthorizedAccessException ex) { return StatusCode(401, new { success = false, message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.Message }); }
        }

        [HttpPost("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> MobileRegister([FromBody] MobileRegisterRequestDto request)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request?.FullName) || string.IsNullOrEmpty(request?.PhoneNumber))
                return BadRequest(new { success = false, message = "Vui lòng cung cấp đầy đủ Họ tên và Số điện thoại" });

            try
            {
                await _mobileService.MobileRegisterAsync(request);
                return Ok(new { success = true, message = "Đăng ký tài khoản thành công!" });
            }
            catch (InvalidOperationException ex) { return BadRequest(new { success = false, message = ex.Message }); }
            catch (Exception ex) { return StatusCode(500, new { success = false, message = "Lỗi hệ thống: " + ex.InnerException?.Message ?? ex.Message }); }
        }
        [HttpGet("AiKnowledge")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAiKnowledge()
        {
            try
            {
                var data = await _mobileService.GetAiKnowledgeBaseAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Lỗi lấy dữ liệu AI: " + ex.Message });
            }
        }
    }
}