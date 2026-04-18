using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class StatisticService
    {
        private readonly IUserRepository _userRepository;
        private readonly IDiagnosticSessionRepository _sessionRepository;
        private readonly IDiagnosisResultRepository _resultRepository;

        public StatisticService(
            IUserRepository userRepository,
            IDiagnosticSessionRepository sessionRepository,
            IDiagnosisResultRepository resultRepository)
        {
            _userRepository = userRepository;
            _sessionRepository = sessionRepository;
            _resultRepository = resultRepository;
        }

        public async Task<DashboardStatsDTO> GetDashboardStatisticsAsync()
        {
            var stats = new DashboardStatsDTO();

            // Hàm CountAsync() của Entity Framework Core
            stats.TotalUsers = await _userRepository.CountAsync();
            stats.TotalDiagnosticSessions = await _sessionRepository.CountAsync();

            // Giả định tầng Repository của bạn có hàm nhận tham số 'limit' để thực hiện .Take(5)
            var topDiseases = await _resultRepository.FindTopDiseasesAsync(5);
            stats.Top5Diseases = (List<TopDiseaseDTO>)topDiseases;

            return stats;
        }
    }
}