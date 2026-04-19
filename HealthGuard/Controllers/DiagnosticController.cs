using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using HealthGuard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    [Authorize]
    public class DiagnosticController : Controller
    {
        private readonly DiagnosticService _diagnosticService;
        private readonly HealthContext _context;

        public DiagnosticController(DiagnosticService diagnosticService, HealthContext context)
        {
            _diagnosticService = diagnosticService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index() => View();

        [HttpGet]
        public IActionResult Details() => View();

        [HttpGet]
        public IActionResult Review() => View();

        [HttpGet]
        public async Task<IActionResult> History()
        {
            string username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username)) return RedirectToAction("Login", "Auth");

            var historyData = await _context.DiagnosticSessions
                .Include(s => s.User)
                .Include(s => s.DiagnosisResults)
                    .ThenInclude(dr => dr.Disease)
                .Where(s => s.User.Username == username)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return View(historyData);
        }

        [HttpGet("api/patient/diagnose/symptoms")]
        public async Task<IActionResult> SelectSymptoms()
        {
            var symptoms = await _context.Symptoms
                .Select(s => new SymptomDto { Id = s.Id, SymptomName = s.SymptomName })
                .ToListAsync();
            return Ok(symptoms);
        }

        [HttpPost("api/patient/diagnose")]
        public async Task<IActionResult> RunDiagnosisAsync(
             [FromBody] DiagnosticRequestDto request,
             [FromServices] IHttpClientFactory httpClientFactory)
        {
            if (request == null || request.SelectedSymptoms == null || request.SelectedSymptoms.Count == 0)
                return BadRequest(new { message = "Vui lòng cung cấp ít nhất một triệu chứng." });

            try
            {
                // 1. GỬI DATA SANG PYTHON
                var pythonPayload = new { selectedSymptoms = request.SelectedSymptoms.Select(s => new { symptomId = s.SymptomId }).ToList() };
                var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var jsonContent = new StringContent(JsonSerializer.Serialize(pythonPayload, jsonOptions), Encoding.UTF8, "application/json");

                var client = httpClientFactory.CreateClient();
                var response = await client.PostAsync("http://127.0.0.1:5000/predict", jsonContent);

                if (!response.IsSuccessStatusCode)
                    return StatusCode(500, new { message = "Lỗi khi gọi mô hình AI Python." });

                // 2. ĐỌC KẾT QUẢ TỪ PYTHON
                var pythonResultString = await response.Content.ReadAsStringAsync();

                // 3. LƯU LỊCH SỬ NGẦM (Bọc Try/Catch để KHÔNG BAO GIỜ LÀM CHẾT GIAO DIỆN)
                // ===================================================================
                // 3. LƯU LỊCH SỬ NGẦM (LƯU TỪNG BƯỚC ĐỂ TRÁNH LỖI "FAILED TO READ RESULT SET")
                // ===================================================================
                try
                {
                    var pythonData = JsonSerializer.Deserialize<PythonAiResponse>(pythonResultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    string username = User.Identity?.Name;
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

                    if (user != null && pythonData?.Diagnoses != null && pythonData.Diagnoses.Count > 0)
                    {
                        // BƯỚC 1: LƯU PHIÊN KHÁM NGAY LẬP TỨC ĐỂ LẤY ID
                        var newSession = new DiagnosticSession { User = user, Status = "Hoàn tất", CreatedAt = DateTime.Now };
                        _context.DiagnosticSessions.Add(newSession);
                        await _context.SaveChangesAsync(); // <--- Lưu phát 1 (Chắc chắn có SessionId)

                        var aiResults = new List<DiagnosisResult>();

                        foreach (var diag in pythonData.Diagnoses)
                        {
                            if (string.IsNullOrEmpty(diag.DiseaseName)) continue;

                            var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.DiseaseName == diag.DiseaseName);

                            if (disease == null)
                            {
                                string generatedCode = "AI-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                                disease = new Disease
                                {
                                    DiseaseCode = generatedCode,
                                    DiseaseName = diag.DiseaseName,
                                    // Cắt bớt chuỗi nếu Python trả về quá dài (phòng hờ lỗi tràn cột DB)
                                    Description = string.IsNullOrEmpty(diag.Description) ? "Đang cập nhật" :
                                                  (diag.Description.Length > 490 ? diag.Description.Substring(0, 490) + "..." : diag.Description),
                                    TreatmentAdvice = string.IsNullOrEmpty(diag.Treatment) ? "Đang cập nhật" :
                                                  (diag.Treatment.Length > 490 ? diag.Treatment.Substring(0, 490) + "..." : diag.Treatment)
                                };

                                _context.Diseases.Add(disease);
                                // BƯỚC 2: LƯU BỆNH MỚI NGAY LẬP TỨC ĐỂ LẤY ID
                                await _context.SaveChangesAsync(); // <--- Lưu phát 2 (Chắc chắn có DiseaseId)
                            }

                            // BƯỚC 3: LẮP RÁP KẾT QUẢ KHI ĐÃ CÓ ĐỦ ID CỦA CHA VÀ CON
                            aiResults.Add(new DiagnosisResult
                            {
                                SessionId = newSession.Id, // Dùng thẳng ID cho chắc ăn
                                DiseaseId = disease.Id,              // Dùng thẳng ID cho chắc ăn
                                ProbabilityPercentage = diag.Probability
                            });
                        }

                        // BƯỚC 4: LƯU KẾT QUẢ CUỐI CÙNG
                        if (aiResults.Count > 0)
                        {
                            _context.DiagnosisResults.AddRange(aiResults);
                            await _context.SaveChangesAsync(); // <--- Lưu phát 3 (Thành công 100%)
                        }
                    }
                }
                catch (Exception dbEx)
                {
                    Console.WriteLine("\n🔥 LỖI LƯU DB: " + dbEx.Message + (dbEx.InnerException != null ? " ---> " + dbEx.InnerException.Message : "") + "\n");
                }

                // 4. TRẢ KẾT QUẢ CHO FRONTEND
                return Content(pythonResultString, "application/json");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi kết nối AI Server: " + ex.Message });
            }
        }
    }
    public class PythonAiResponse
    {
        public string Status { get; set; }
        public List<PythonDiagnosis> Diagnoses { get; set; }
    }

    public class PythonDiagnosis
    {
        public string DiseaseName { get; set; }
        public double Probability { get; set; }
        public string Description { get; set; }
        public string Treatment { get; set; }
    }
}