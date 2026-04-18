using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Models.Entity;
using HealthGuard.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class DiagnosticService
    {
        private readonly IUserRepository _userRepository;
        private readonly ISymptomRepository _symptomRepository;
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly IDiagnosticSessionRepository _sessionRepository;
        private readonly ISessionSymptomRepository _sessionSymptomRepository;
        private readonly IDiagnosisResultRepository _resultRepository;

        // Tiêm IHttpClientFactory để gọi API ngoại vi
        private readonly IHttpClientFactory _httpClientFactory;
        private const string PYTHON_API_URL = "http://localhost:5000/api/ai-chat";

        public DiagnosticService(
            IUserRepository userRepository,
            ISymptomRepository symptomRepository,
            IDiseaseRepository diseaseRepository,
            IDiagnosticSessionRepository sessionRepository,
            ISessionSymptomRepository sessionSymptomRepository,
            IDiagnosisResultRepository resultRepository,
            IHttpClientFactory httpClientFactory)
        {
            _userRepository = userRepository;
            _symptomRepository = symptomRepository;
            _diseaseRepository = diseaseRepository;
            _sessionRepository = sessionRepository;
            _sessionSymptomRepository = sessionSymptomRepository;
            _resultRepository = resultRepository;
            _httpClientFactory = httpClientFactory;
        }

        // Entity Framework Core sẽ tự quản lý transaction trong một Request Scope
        // khi bạn gọi SaveChangesAsync() ở cuối, nên không cần [Transactional]
        public async Task<DiagnosticResponseDTO> PerformDiagnosisAsync(string username, DiagnosticRequestDTO request)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null)
            {
                throw new Exception("Không tìm thấy người dùng!");
            }

            var session = new DiagnosticSession
            {
                User = user,
                Status = "COMPLETED"
            };
            session = await _sessionRepository.SaveAsync(session);

            foreach (var input in request.SelectedSymptoms)
            {
                var symptom = await _symptomRepository.FindByIdAsync(input.SymptomId);
                if (symptom == null)
                {
                    throw new Exception($"Triệu chứng không tồn tại với ID: {input.SymptomId}");
                }

                var sessionSymptom = new SessionSymptom
                {
                    DiagnosticSession = session,
                    Symptom = symptom,
                    DurationDays = input.DurationDays,
                    SeverityLevel = input.SeverityLevel
                };
                await _sessionSymptomRepository.SaveAsync(sessionSymptom);
            }

            // Gọi server AI Python
            var pythonResults = await CallPythonServiceAsync(request);

            var finalResults = new List<ResultResponseDTO>();

            foreach (var pyResult in pythonResults)
            {
                var disease = await _diseaseRepository.FindByDiseaseCodeAsync(pyResult.DiseaseCode);
                if (disease == null)
                {
                    throw new Exception($"Máy chủ AI trả về mã bệnh không xác định: {pyResult.DiseaseCode}");
                }

                var result = new DiagnosisResult
                {
                    DiagnosticSession = session,
                    Disease = disease,
                    ProbabilityPercentage = pyResult.Probability
                };
                await _resultRepository.SaveAsync(result);

                finalResults.Add(new ResultResponseDTO(
                    disease.DiseaseName,
                    pyResult.Probability,
                    disease.TreatmentAdvice
                ));
            }

            return new DiagnosticResponseDTO(session.Id, finalResults);
        }

        private async Task<List<PythonResponseDTO>> CallPythonServiceAsync(DiagnosticRequestDTO request)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                // Tự động serialize request sang JSON và gửi POST
                var response = await client.PostAsJsonAsync(PYTHON_API_URL, request);

                // Ném lỗi nếu status code không phải 2xx
                response.EnsureSuccessStatusCode();

                // Đọc JSON trả về và deserialize
                var result = await response.Content.ReadFromJsonAsync<PythonResponseDTO[]>();
                return new List<PythonResponseDTO>(result);
            }
            catch (Exception ex)
            {
                // Logic mock khi server Python tắt
                /*
                return new List<PythonResponseDTO>
                {
                    new PythonResponseDTO { DiseaseCode = "J00", Probability = 85.5f }
                };
                */
                throw new Exception($"Lỗi kết nối với máy chủ AI (Python): {ex.Message}");
            }
        }
    }
}