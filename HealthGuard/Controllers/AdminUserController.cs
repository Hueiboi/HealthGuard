using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;
using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    // [Authorize(Roles = "ROLE_ADMIN")] 
    public class AdminUserController : ControllerBase
    {
        private readonly AdminUserService _adminUserService;

        public AdminUserController(AdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsersAsync(
            [FromQuery] int page = 0,
            [FromQuery] int size = 10,
            [FromQuery] string keyword = null)
        {
            var users = await _adminUserService.GetAllUsersAsync(page, size, keyword);
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] int id) // Đổi long -> int
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatusAsync(
            [FromRoute] int id, // Đổi long -> int
            [FromBody] Dictionary<string, bool> statusUpdate)
        {
            if (!statusUpdate.TryGetValue("isActive", out bool isActive))
            {
                return BadRequest(new { message = "Thiếu trường isActive trong payload." });
            }

            var updatedUser = await _adminUserService.UpdateUserStatusAsync(id, isActive);
            return Ok(updatedUser);
        }

        [HttpPut("{id}/role/{roleId}")]
        public async Task<IActionResult> ChangeUserRoleAsync(
            [FromRoute] int id, // Đổi long -> int
            [FromRoute] int roleId) // Đổi long -> int
        {
            var updatedUser = await _adminUserService.ChangeUserRoleAsync(id, roleId);
            return Ok(updatedUser);
        }
    }
}