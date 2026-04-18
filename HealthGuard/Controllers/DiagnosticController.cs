using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/patient/diagnose")]
    [Authorize] // Bắt buộc đăng nhập
    public class DiagnosticController : ControllerBase
    {
        private readonly DiagnosticService _diagnosticService;

        public DiagnosticController(DiagnosticService diagnosticService)
        {
            _diagnosticService = diagnosticService;
        }

        [HttpPost]
        public async Task<IActionResult> RunDiagnosisAsync([FromBody] DiagnosticRequestDTO request)
        {
            // Lấy username từ JWT Token
            string username = User.Identity.Name;

            var result = await _diagnosticService.PerformDiagnosisAsync(username, request);
            return Ok(result);
        }
    }
}