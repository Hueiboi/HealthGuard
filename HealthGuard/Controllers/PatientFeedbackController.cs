using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/patient/feedbacks")]
    [Authorize]
    public class PatientFeedbackController : ControllerBase
    {
        private readonly PatientFeedbackService _feedbackService;

        public PatientFeedbackController(PatientFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFeedbackAsync([FromBody] FeedbackRequestDto request) // Sửa DTO -> Dto
        {
            string username = User.Identity.Name;
            var result = await _feedbackService.SubmitFeedbackAsync(username, request);

            return Ok(result);
        }
    }
}