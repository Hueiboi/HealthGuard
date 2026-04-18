using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/patient/history")]
    [Authorize]
    public class PatientHistoryController : ControllerBase
    {
        private readonly PatientHistoryService _historyService;

        public PatientHistoryController(PatientHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyHistoryAsync(
            [FromQuery] int page = 0,
            [FromQuery] int size = 10)
        {
            string username = User.Identity.Name;
            var history = await _historyService.GetMyHistoryListAsync(username, page, size);
            return Ok(history);
        }

        [HttpGet("{sessionId}")]
        public async Task<IActionResult> GetHistoryDetailAsync([FromRoute] int sessionId) // Đổi long -> int
        {
            string username = User.Identity.Name;
            var detail = await _historyService.GetHistoryDetailAsync(username, sessionId);
            return Ok(detail);
        }
    }
}