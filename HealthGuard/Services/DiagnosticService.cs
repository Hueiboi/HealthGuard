using HealthGuard.Data; // Đảm bảo namespace này chứa HealthContext của ông
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity; 
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class DiagnosticService
    {
        private readonly HealthContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string PYTHON_API_URL = "http://localhost:5000/api/ai-chat";

        public DiagnosticService(HealthContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<DiagnosticResponseDto> PerformDiagnosisAsync(string username, DiagnosticRequestDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null) throw new UnauthorizedAccessException("Không tìm thấy người dùng!");

 
            var session = new DiagnosticSession
            {
                User = user,
                Status = "COMPLETED",
                CreatedAt = DateTime.UtcNow
            };
            _context.DiagnosticSessions.Add(session);

            foreach (var input in request.SelectedSymptoms)
            {
                var symptom = await _context.Symptoms.FindAsync(input.SymptomId);
                if (symptom == null) throw new KeyNotFoundException($"Lỗi ID triệu chứng: {input.SymptomId}");

                var sessionSymptom = new SessionSymptom
                {
                    DiagnosticSession = session,
                    Symptom = symptom,
                    DurationDays = input.DurationDays,
                    SeverityLevel = input.SeverityLevel
                };
                _context.SessionSymptoms.Add(sessionSymptom);
            }

            var pythonResults = await CallPythonServiceAsync(request);

            var finalResults = new List<ResultResponseDto>();
            foreach (var pyResult in pythonResults)
            {
                var disease = await _context.Diseases.FirstOrDefaultAsync(d => d.DiseaseCode == pyResult.DiseaseCode);
                if (disease == null) continue;

                var result = new DiagnosisResult
                {
                    DiagnosticSession = session,
                    Disease = disease,
                    ProbabilityPercentage = pyResult.Probability
                };
                _context.DiagnosisResults.Add(result);

                finalResults.Add(new ResultResponseDto
                {
                    DiseaseName = disease.DiseaseName,
                    ProbabilityPercentage = pyResult.Probability,
                    TreatmentAdvice = disease.TreatmentAdvice
                });
            }

            await _context.SaveChangesAsync();

            return new DiagnosticResponseDto
            {
                SessionId = session.Id,
                Results = finalResults
            };
        }

        private async Task<List<PythonResponseDto>> CallPythonServiceAsync(DiagnosticRequestDto request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.PostAsJsonAsync(PYTHON_API_URL, request);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<PythonResponseDto[]>();
                return new List<PythonResponseDto>(result ?? Array.Empty<PythonResponseDto>());
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi AI: {ex.Message}");
            }
        }
    }
}