using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;
using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/disease")]
    // [Authorize(Roles = "ROLE_ADMIN")] 
    public class DiseaseController : ControllerBase
    {
        private readonly DiseaseService _diseaseService;

        public DiseaseController(DiseaseService diseaseService)
        {
            _diseaseService = diseaseService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiseaseAsync([FromBody] DiseaseDTO request) // Bạn định nghĩa là DiseaseDTO (viết hoa)
        {
            var createdDisease = await _diseaseService.CreateDiseaseAsync(request);
            return StatusCode(201, createdDisease);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDiseasesAsync(
            [FromQuery] int page = 0,
            [FromQuery] int size = 10,
            [FromQuery] string keyword = null)
        {
            var diseasePage = await _diseaseService.GetAllDiseasesAsync(page, size, keyword);
            return Ok(diseasePage);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiseaseByIdAsync([FromRoute] int id) // Đổi long -> int
        {
            var disease = await _diseaseService.GetDiseaseByIdAsync(id);
            return Ok(disease);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiseaseAsync([FromRoute] int id, [FromBody] DiseaseDTO request) // Đổi long -> int
        {
            var updatedDisease = await _diseaseService.UpdateDiseaseAsync(id, request);
            return Ok(updatedDisease);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiseaseAsync([FromRoute] int id) // Đổi long -> int
        {
            await _diseaseService.DeleteDiseaseAsync(id);
            return NoContent();
        }
    }
}