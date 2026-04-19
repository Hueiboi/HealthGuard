using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;
using HealthGuard.Models.Dto;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/disease")]
    // [Authorize(Roles = "ROLE_ADMIN")] // Mở comment nếu bạn muốn chặn user thường
    public class DiseaseController : ControllerBase
    {
        private readonly DiseaseService _diseaseService;

        public DiseaseController(DiseaseService diseaseService)
        {
            _diseaseService = diseaseService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDiseaseAsync([FromBody] DiseaseDto request)
        {
            var createdDisease = await _diseaseService.CreateDiseaseAsync(request);
            return StatusCode(201, createdDisease); // Trả về HttpStatus.CREATED
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
        public async Task<IActionResult> GetDiseaseByIdAsync([FromRoute] long id)
        {
            var disease = await _diseaseService.GetDiseaseByIdAsync(id);
            return Ok(disease);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDiseaseAsync([FromRoute] long id, [FromBody] DiseaseDto request)
        {
            var updatedDisease = await _diseaseService.UpdateDiseaseAsync(id, request);
            return Ok(updatedDisease);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDiseaseAsync([FromRoute] long id)
        {
            await _diseaseService.DeleteDiseaseAsync(id);
            return NoContent();
        }
    }
}