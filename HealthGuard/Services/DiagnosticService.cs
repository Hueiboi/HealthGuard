using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class DiagnosticService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDiagnosticSessionRepository _sessionRepository;
        private readonly ISymptomRepository _symptomRepository;
        private readonly ISessionSymptomRepository _sessionSymptomRepository;
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly IDiagnosisResultRepository _resultRepository;

        public DiagnosticService(
            IUserRepository userRepository,
            IDiagnosticSessionRepository sessionRepository,
            ISymptomRepository symptomRepository,
            ISessionSymptomRepository sessionSymptomRepository,
            IDiseaseRepository diseaseRepository,
            IDiagnosisResultRepository resultRepository)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _symptomRepository = symptomRepository;
            _sessionSymptomRepository = sessionSymptomRepository;
            _diseaseRepository = diseaseRepository;
            _resultRepository = resultRepository;
        }

        public async Task<DiagnosticResponseDto> PerformDiagnosisAsync(string username, DiagnosticRequestDto request)
        {
            var user = await _userRepository.FindByUsernameAsync(username);
            if (user == null) throw new Exception("Không tìm thấy người dùng!");

            var session = new DiagnosticSession
            {
                User = user,
                CreatedAt = DateTime.Now
            };
            session = await _sessionRepository.SaveAsync(session);

            foreach (var input in request.Symptoms)
            {
                var symptom = await _symptomRepository.FindByIdAsync(input.SymptomId);
                if (symptom == null) throw new Exception($"Triệu chứng ID {input.SymptomId} không tồn tại");

                var sessionSymptom = new SessionSymptom
                {
                    DiagnosticSession = session,
                    Symptom = symptom,
                    DurationDays = input.DurationDays,
                    Severity = input.Severity
                };
                await _sessionSymptomRepository.SaveAsync(sessionSymptom);
            }

            // Giả định CallPythonServiceAsync đã được định nghĩa và trả về dữ liệu
            var pythonResults = new List<PythonResponseDto>();

            var finalResults = new List<ResultResponseDto>();
            foreach (var pyResult in pythonResults)
            {
                // Logic giả định khi gọi Python thành công...
            }
            return new DiagnosticResponseDto();
        }
    }
}