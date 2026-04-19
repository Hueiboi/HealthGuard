using HealthGuard.Data;
using HealthGuard.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class PatientHistoryService
    {
        private readonly HealthContext _context;

        public PatientHistoryService(HealthContext context)
        {
            _context = context;
        }

        // Lấy danh sách tổng quan các lần khám
        public async Task<IEnumerable<HistoryListDto>> GetMyHistoryListAsync(string username, int page, int size)
        {
            // Dùng LINQ để phân trang và map DTO ngay trên SQL
            return await _context.DiagnosticSessions
                .Where(s => s.User.Username == username)
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * size)
                .Take(size)
                .Select(session => new HistoryListDto
                {
                    SessionId = session.Id,
                    CreatedAt = session.CreatedAt,
                    Status = session.Status
                })
                .ToListAsync();
        }

        // Lấy chi tiết 1 lần khám
        public async Task<HistoryDetailDto> GetHistoryDetailAsync(string username, long sessionId)
        {
            // Dùng Include và ThenInclude để lấy sạch dữ liệu liên quan trong 1 nốt nhạc
            var session = await _context.DiagnosticSessions
                .Include(s => s.SessionSymptoms).ThenInclude(ss => ss.Symptom)
                .Include(s => s.DiagnosisResults).ThenInclude(dr => dr.Disease)
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.User.Username == username);

            if (session == null)
            {
                throw new UnauthorizedAccessException("Không tìm thấy dữ liệu hoặc bạn không có quyền xem lịch sử này!");
            }

            return new HistoryDetailDto
            {
                SessionId = session.Id,
                CreatedAt = session.CreatedAt,
                Symptoms = session.SessionSymptoms.Select(s => new HistoryDetailDto.SymptomItem
                {
                    SymptomName = s.Symptom.SymptomName,
                    DurationDays = s.DurationDays,
                    SeverityLevel = s.SeverityLevel
                }).ToList(),
                Results = session.DiagnosisResults.Select(r => new HistoryDetailDto.ResultItem
                {
                    DiseaseName = r.Disease.DiseaseName,
                    ProbabilityPercentage = (float)r.ProbabilityPercentage,
                    TreatmentAdvice = r.Disease.TreatmentAdvice
                }).ToList()
            };
        }
    }
}