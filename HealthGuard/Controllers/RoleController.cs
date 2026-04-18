using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/roles")]
    // [Authorize(Roles = "ROLE_ADMIN")]
    public class RoleController : ControllerBase
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoleAsync([FromBody] RoleDto request) // Sửa DTO -> Dto
        {
            var createdRole = await _roleService.CreateRoleAsync(request);
            return StatusCode(201, createdRole);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleByIdAsync([FromRoute] int id) // Đổi long -> int
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleAsync([FromRoute] int id, [FromBody] RoleDto request) // Đổi long -> int
        {
            var updatedRole = await _roleService.UpdateRoleAsync(id, request);
            return Ok(updatedRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleAsync([FromRoute] int id) // Đổi long -> int
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent();
        }
    }
}