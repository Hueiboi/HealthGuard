using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    // [Authorize(Roles = "ROLE_ADMIN")] // Mở comment để phân quyền Admin
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
        public async Task<IActionResult> GetUserByIdAsync([FromRoute] long id)
        {
            var user = await _adminUserService.GetUserByIdAsync(id);
            return Ok(user);
        }

        // Tương đương với @PatchMapping trong Java
        // Map<String, Boolean> của Java được chuyển thành Dictionary<string, bool> trong C#
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateUserStatusAsync(
            [FromRoute] long id,
            [FromBody] Dictionary<string, bool> statusUpdate)
        {
            // Kiểm tra xem key "isActive" có tồn tại trong Dictionary không
            if (!statusUpdate.TryGetValue("isActive", out bool isActive))
            {
                return BadRequest(new { message = "Thiếu trường isActive trong payload." });
            }

            var updatedUser = await _adminUserService.UpdateUserStatusAsync(id, isActive);
            return Ok(updatedUser);
        }

        [HttpPut("{id}/role/{roleId}")]
        public async Task<IActionResult> ChangeUserRoleAsync(
            [FromRoute] long id,
            [FromRoute] long roleId)
        {
            var updatedUser = await _adminUserService.ChangeUserRoleAsync(id, roleId);
            return Ok(updatedUser);
        }
    }
}