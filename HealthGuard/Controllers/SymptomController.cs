using HealthGuard.Models.Dto; // Đã sửa DTOs -> Dto
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/symptom")]
    // [Authorize(Roles = "ROLE_ADMIN")]
    public class SymptomController : ControllerBase
    {
        private readonly SymptomService _symptomService;

        public SymptomController(SymptomService symptomService)
        {
            _symptomService = symptomService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSymptomAsync([FromBody] SymptomDto request) // Sửa DTO -> Dto
        {
            var createdSymptom = await _symptomService.CreateSymptomAsync(request);
            return StatusCode(201, createdSymptom);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSymptomsAsync(
            [FromQuery] int page = 0,
            [FromQuery] int size = 10,
            [FromQuery] string keyword = null)
        {
            var symptoms = await _symptomService.GetAllSymptomsAsync(page, size, keyword);
            return Ok(symptoms);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSymptomByIdAsync([FromRoute] int id) // Đổi long -> int
        {
            var symptom = await _symptomService.GetSymptomByIdAsync(id);
            return Ok(symptom);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSymptomAsync([FromRoute] int id, [FromBody] SymptomDto request) // Đổi long -> int
        {
            var updatedSymptom = await _symptomService.UpdateSymptomAsync(id, request);
            return Ok(updatedSymptom);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSymptomAsync([FromRoute] int id) // Đổi long -> int
        {
            await _symptomService.DeleteSymptomAsync(id);
            return NoContent();
        }

        [HttpPost("weights")]
        public async Task<IActionResult> AssignWeightAsync([FromBody] WeightRuleDto rule) // Sửa DTO -> Dto
        {
            await _symptomService.AssignWeightScoreAsync(rule);
            return Ok(new { message = "Đã thiết lập trọng số thành công!" });
        }

        [HttpGet("weights/disease/{diseaseId}")]
        public async Task<IActionResult> GetWeightsByDiseaseAsync([FromRoute] int diseaseId) // Đổi long -> int
        {
            var weights = await _symptomService.GetSymptomsByDiseaseAsync(diseaseId);
            return Ok(weights);
        }
    }
}