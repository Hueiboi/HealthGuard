using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/patient/profile")]
    [Authorize]
    public class PatientProfileController : ControllerBase
    {
        private readonly PatientProfileService _patientProfileService;

        public PatientProfileController(PatientProfileService patientProfileService)
        {
            _patientProfileService = patientProfileService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyProfileAsync()
        {
            string currentUsername = User.Identity.Name;
            var profile = await _patientProfileService.GetMyProfileAsync(currentUsername);
            return Ok(profile);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfileAsync([FromBody] PatientProfileDto request)
        {
            string currentUsername = User.Identity.Name;
            var updatedProfile = await _patientProfileService.UpdateProfileAsync(request, currentUsername);
            return Ok(updatedProfile);
        }
    }
}