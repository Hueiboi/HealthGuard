using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;
using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/feedbacks")]
    // [Authorize(Roles = "ROLE_ADMIN")]
    public class AdminFeedbackController : ControllerBase
    {
        private readonly AdminFeedbackService _adminFeedbackService;

        public AdminFeedbackController(AdminFeedbackService adminFeedbackService)
        {
            _adminFeedbackService = adminFeedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacksAsync(
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            var feedbacks = await _adminFeedbackService.GetAllFeedbacksAsync(page, size);
            return Ok(feedbacks);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackByIdAsync([FromRoute] int id) // Đổi long -> int
        {
            var feedback = await _adminFeedbackService.GetFeedbackByIdAsync(id);
            return Ok(feedback);
        }
    }
}