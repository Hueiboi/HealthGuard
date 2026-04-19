using HealthGuard.Models.Dto;
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
        public async Task<IActionResult> CreateRoleAsync([FromBody] RoleDto request)
        {
            var createdRole = await _roleService.CreateRoleAsync(request);
            // HttpStatus.CREATED (201)
            return StatusCode(201, createdRole);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRolesAsync()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(roles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleByIdAsync([FromRoute] long id)
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoleAsync([FromRoute] long id, [FromBody] RoleDto request)
        {
            var updatedRole = await _roleService.UpdateRoleAsync(id, request);
            return Ok(updatedRole);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoleAsync([FromRoute] long id)
        {
            await _roleService.DeleteRoleAsync(id);
            return NoContent(); // Tương đương ResponseEntity.noContent().build()
        }
    }
}