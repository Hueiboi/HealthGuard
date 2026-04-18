using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto
using HealthGuard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDto request) // Sửa DTO -> Dto
        {
            var registeredUser = await _authService.RegisterAsync(request);
            return StatusCode(201, registeredUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDto request) // Sửa DTO -> Dto
        {
            var token = await _authService.LoginAsync(request);
            return Ok(new { token = token });
        }
    }
}