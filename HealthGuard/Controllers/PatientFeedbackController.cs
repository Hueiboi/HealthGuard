using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic; // Để xài KeyNotFoundException
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/patient/feedbacks")]
    [Authorize]
    public class PatientFeedbackController : Controller
    {
        private readonly PatientFeedbackService _feedbackService;

        public PatientFeedbackController(PatientFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }
        // Action này để trả về trang Giao diện (View)
        [HttpGet("/patient/messages")]
        public async Task<IActionResult> Messages()
        {
            string username = User.Identity.Name;
            var feedbacks = await _feedbackService.GetUserFeedbacksAsync(username);
            return View(feedbacks); // Trả về file Messages.cshtml
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedbackAsync([FromBody] FeedbackRequestDto request)
        {
            // Bắt lỗi cơ bản ngay từ cửa
            if (request == null || string.IsNullOrWhiteSpace(request.Comments))
            {
                return BadRequest(new { message = "Dữ liệu phản hồi không hợp lệ hoặc bị trống." });
            }

            try
            {
                string username = User.Identity?.Name;
                var result = await _feedbackService.SubmitFeedbackAsync(username, request);

                // Trả về 200 OK kèm dữ liệu
                return Ok(new { message = "Phản hồi đã được gửi thành công!", data = result });
            }
            catch (UnauthorizedAccessException ex)
            {
                // Trả về 401 nếu user tào lao
                return StatusCode(401, new { message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về 404 nếu không tìm thấy Session khám
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Trả về 500 cho các lỗi rớt mạng DB,...
                return StatusCode(500, new { message = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}