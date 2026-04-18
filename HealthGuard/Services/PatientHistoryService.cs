using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientHistoryService
    {
        private readonly IDiagnosticSessionRepository _sessionRepository;
        private readonly ISessionSymptomRepository _sessionSymptomRepository;
        private readonly IDiagnosisResultRepository _diagnosisResultRepository;

        public PatientHistoryService(
            IDiagnosticSessionRepository sessionRepository,
            ISessionSymptomRepository sessionSymptomRepository,
            IDiagnosisResultRepository diagnosisResultRepository)
        {
            _sessionRepository = sessionRepository;
            _sessionSymptomRepository = sessionSymptomRepository;
            _diagnosisResultRepository = diagnosisResultRepository;
        }

        public async Task<IEnumerable<HistoryListDto>> GetMyHistoryListAsync(string username, int page, int size)
        {
            var sessions = await _sessionRepository.FindByUserUsernameOrderByCreatedAtDescAsync(username, page, size);

            var dtos = new List<HistoryListDto>();
            foreach (var session in sessions)
            {
                dtos.Add(new HistoryListDto()); // Tùy biến sau
            }
            return dtos;
        }

        public async Task<HistoryDetailDto> GetHistoryDetailAsync(string username, int sessionId) // Đổi long -> int
        {
            var session = await _sessionRepository.FindByIdAndUserUsernameAsync(sessionId, username);
            if (session == null) throw new Exception("Không tìm thấy dữ liệu hoặc bạn không có quyền xem!");

            return new HistoryDetailDto(); // Tùy biến sau
        }
    }
}