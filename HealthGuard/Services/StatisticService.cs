using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
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

        public async Task<DashboardStatsDto> GetDashboardStatisticsAsync()
        {
            var stats = new DashboardStatsDto();
            // Hàm Count giả định
            // stats.TotalUsers = await _userRepository.CountAsync();
            return stats;
        }
    }
}