using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Repositories;
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

        // Lấy danh sách tổng quan các lần khám
        public async Task<IEnumerable<HistoryListDTO>> GetMyHistoryListAsync(string username, int page, int size)
        {
            // Logic OrderByDescending(CreatedAt) và phân trang thực hiện ở Repository
            var sessions = await _sessionRepository.FindByUserUsernameOrderByCreatedAtDescAsync(username, page, size);

            var dtos = new List<HistoryListDTO>();
            foreach (var session in sessions)
            {
                dtos.Add(new HistoryListDTO
                {
                    SessionId = session.Id,
                    CreatedAt = session.CreatedAt,
                    Status = session.Status
                });
            }
            return dtos;
        }

        // Lấy chi tiết 1 lần khám
        public async Task<HistoryDetailDTO> GetHistoryDetailAsync(string username, long sessionId)
        {
            // Kiểm tra an toàn: Lấy session dựa trên cả ID và Username để chống xem trộm
            var session = await _sessionRepository.FindByIdAndUserUsernameAsync(sessionId, username);
            if (session == null)
            {
                throw new Exception("Không tìm thấy dữ liệu hoặc bạn không có quyền xem lịch sử này!");
            }

            var detailDTO = new HistoryDetailDTO
            {
                SessionId = session.Id,
                CreatedAt = session.CreatedAt,
                Symptoms = new List<HistoryDetailDTO.SymptomItem>(),
                Results = new List<HistoryDetailDTO.ResultItem>()
            };

            // Lấy danh sách triệu chứng đã chọn
            var symptoms = await _sessionSymptomRepository.FindByDiagnosticSessionIdAsync(sessionId);
            foreach (var s in symptoms)
            {
                detailDTO.Symptoms.Add(new HistoryDetailDTO.SymptomItem
                {
                    // Đảm bảo navigation property Symptom đã được Include() ở Repository
                    SymptomName = s.Symptom.SymptomName,
                    DurationDays = s.DurationDays,
                    SeverityLevel = s.SeverityLevel
                });
            }

            // Lấy danh sách kết quả bệnh
            var results = await _diagnosisResultRepository.FindByDiagnosticSessionIdAsync(sessionId);
            foreach (var r in results)
            {
                detailDTO.Results.Add(new HistoryDetailDTO.ResultItem
                {
                    // Đảm bảo navigation property Disease đã được Include() ở Repository
                    DiseaseName = r.Disease.DiseaseName,
                    ProbabilityPercentage = r.ProbabilityPercentage,
                    TreatmentAdvice = r.Disease.TreatmentAdvice
                });
            }

            return detailDTO;
        }
    }
}