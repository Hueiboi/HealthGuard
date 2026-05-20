using HealthGuard.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Route("api/Web")]
    [ApiController]
    public class AiWebController : ControllerBase
    {
        private readonly HealthContext _context;

        public AiWebController(HealthContext context)
        {
            _context = context;
        }

        [HttpGet("AiKnowledge")]
        public async Task<IActionResult> GetAiKnowledge()
        {
            try
            {
                var diseases = await _context.Diseases
                    .Include(d => d.DiseaseSymptoms)
                    .ToListAsync();

                var result = diseases.Select(d => new
                {
                    diseaseName = d.DiseaseName,
                    description = string.IsNullOrEmpty(d.Description) ? "Thông tin bệnh lý đang được cập nhật." : d.Description,
                    treatment = string.IsNullOrEmpty(d.TreatmentAdvice) ? "Vui lòng tham khảo ý kiến bác sĩ chuyên khoa." : d.TreatmentAdvice,

                    weights = d.DiseaseSymptoms.ToDictionary(
                        ds => ds.SymptomId.ToString(),
                        ds => ds.WeightScore
                    )
                }).ToList();

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi trích xuất dữ liệu Y khoa: " + ex.Message });
            }
        }
    }
}