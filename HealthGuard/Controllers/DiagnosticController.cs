using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    public class DiagnosticController : Controller
    {
        private readonly DiagnosticService _diagnosticService;
        private readonly HealthContext _context; // Khai báo Context để gọi DB

        public DiagnosticController(DiagnosticService diagnosticService, HealthContext context)
        {
            _diagnosticService = diagnosticService;
            _context = context;
        }

        // ==========================================
        // 1. ACTION TRẢ VỀ GIAO DIỆN (VIEW)
        // ==========================================

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Details()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Review()
        {
            return View();
        }

        // ==========================================
        // 2. CÁC API TRẢ VỀ VÀ XỬ LÝ DỮ LIỆU (JSON)
        // ==========================================

        // Lấy danh sách triệu chứng TỪ DATABASE
        [HttpGet("api/patient/diagnose/symptoms")]
        // [Authorize] // Bật lên sau khi test xong
        public async Task<IActionResult> SelectSymptoms()
        {
            // Truy vấn trực tiếp từ bảng Symptoms dưới MySQL
            var symptoms = await _context.Symptoms
                .Select(s => new SymptomDto
                {
                    Id = s.Id,
                    SymptomName = s.SymptomName
                })
                .ToListAsync();

            return Ok(symptoms);
        }

        // Đẩy ID triệu chứng sang Python và nhận kết quả
        [HttpPost("api/patient/diagnose")]
        // [Authorize]
        public async Task<IActionResult> RunDiagnosisAsync(
             [FromBody] DiagnosticRequestDto request,
             [FromServices] IHttpClientFactory httpClientFactory)
        {
            if (request == null || request.SelectedSymptoms == null || request.SelectedSymptoms.Count == 0)
            {
                return BadRequest(new { message = "Vui lòng cung cấp ít nhất một triệu chứng." });
            }

            try
            {
                var pythonPayload = new
                {
                    selectedSymptoms = request.SelectedSymptoms.Select(s => new { symptomId = s.SymptomId }).ToList()
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(pythonPayload, jsonOptions),
                    Encoding.UTF8,
                    "application/json"
                );

                // 2. GỌI SANG SERVER PYTHON 
                var client = httpClientFactory.CreateClient();
                var response = await client.PostAsync("http://127.0.0.1:5000/predict", jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode(500, new { message = "Lỗi khi gọi mô hình AI Python." });
                }

                // 3. ĐỌC KẾT QUẢ TỪ PYTHON
                var pythonResultString = await response.Content.ReadAsStringAsync();

                // 4. TRẢ THẲNG CHO FRONTEND
                // Vì Python đã trả về format JSON chuẩn { "status": "Success", "diagnoses": [...] } 
                // C# chỉ việc đẩy thẳng cục này cho trang Review.cshtml render ra màn hình.
                return Content(pythonResultString, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi kết nối AI Server: " + ex.Message });
            }
        }
    }
}