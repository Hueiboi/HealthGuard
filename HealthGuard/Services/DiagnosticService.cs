using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class DiagnosticService
    {
        private readonly HealthContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string PYTHON_API_URL = "http://127.0.0.1:5000/predict";

        public DiagnosticService(HealthContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<object> PerformDiagnosisAsync(string username, DiagnosticRequestDto request)
        {
            string cleanUsername = username.Trim().ToLower();

            // 🔥 ĐÃ FIX: Chống lỗi Cookie bóng ma bằng cách tìm theo cả Username lẫn Email
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username.ToLower() == cleanUsername ||
                u.Email.ToLower() == cleanUsername);

            if (user == null)
                throw new Exception($"Không tìm thấy tài khoản [{username}] trong Database! Vui lòng ấn Đăng xuất và Đăng nhập lại.");

            var pythonData = await CallPythonAiAsync(request);

            if (pythonData?.Diagnoses == null || !pythonData.Diagnoses.Any())
                return new { status = "Hoàn tất", diagnoses = new List<PythonDiagnosis>() };

            // 🔥 CHUẨN Y KHOA: Chỉ lấy các bệnh > 12%, KHÔNG ÉP TỔNG 100%
            var matchedDiagnoses = pythonData.Diagnoses
                .Where(d => d.Probability > 12.0)
                .OrderByDescending(d => d.Probability)
                .Take(5)
                .ToList();

            if (!matchedDiagnoses.Any())
                return new { status = "Hoàn tất", diagnoses = new List<PythonDiagnosis>() };

            await SaveDiagnosisToDb(user, matchedDiagnoses);

            return new { status = "Hoàn tất", diagnoses = matchedDiagnoses };
        }

        private async Task<PythonAiResponse> CallPythonAiAsync(DiagnosticRequestDto request)
        {
            var client = _httpClientFactory.CreateClient();
            var pythonPayload = new { selectedSymptoms = request.SelectedSymptoms.Select(s => new { symptomId = s.SymptomId }).ToList() };
            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonContent = new StringContent(JsonSerializer.Serialize(pythonPayload, jsonOptions), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(PYTHON_API_URL, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var errorTxt = await response.Content.ReadAsStringAsync();
                throw new Exception($"Lỗi Python AI: {errorTxt}");
            }

            var resultString = await response.Content.ReadAsStringAsync();
            try
            {
                return JsonSerializer.Deserialize<PythonAiResponse>(resultString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi đọc JSON từ Python: {ex.Message}");
            }
        }

        private async Task SaveDiagnosisToDb(User user, List<PythonDiagnosis> diagnoses)
        {
            var newSession = new DiagnosticSession { User = user, Status = "Hoàn tất", CreatedAt = DateTime.Now };
            _context.DiagnosticSessions.Add(newSession);
            await _context.SaveChangesAsync();

            foreach (var diag in diagnoses)
            {
                var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.DiseaseName == diag.DiseaseName);

                if (disease == null)
                {
                    disease = new Disease
                    {
                        DiseaseCode = "AI-" + Guid.NewGuid().ToString().Substring(0, 6).ToUpper(),
                        DiseaseName = diag.DiseaseName,
                        Description = diag.Description ?? "Đang cập nhật",
                        TreatmentAdvice = diag.Treatment ?? "Đang cập nhật"
                    };
                    _context.Diseases.Add(disease);
                    await _context.SaveChangesAsync();
                }

                _context.DiagnosisResults.Add(new DiagnosisResult
                {
                    SessionId = newSession.Id,
                    DiseaseId = disease.Id,
                    ProbabilityPercentage = diag.Probability
                });
            }
            await _context.SaveChangesAsync();
        }
    }

    public class PythonAiResponse { public string Status { get; set; } public List<PythonDiagnosis> Diagnoses { get; set; } }
    public class PythonDiagnosis { public string DiseaseName { get; set; } public double Probability { get; set; } public string Description { get; set; } public string Treatment { get; set; } }
}