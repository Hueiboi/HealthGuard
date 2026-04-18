using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
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

        // Endpoint: POST /api/auth/register
        [HttpPost("register")]
        public async Task<IActionResult> RegisterUserAsync([FromBody] RegisterRequestDTO request)
        {
            var registeredUser = await _authService.RegisterAsync(request);
            return StatusCode(201, registeredUser); // HttpStatus.CREATED
        }

        // Endpoint: POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> LoginUserAsync([FromBody] LoginRequestDTO request)
        {
            var token = await _authService.LoginAsync(request);
            // Trả về chuỗi Token dạng JSON thay vì chuỗi trần để Frontend dễ Parse
            return Ok(new { token = token });
        }
    }
}