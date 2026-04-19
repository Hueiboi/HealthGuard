using HealthGuard.Data;
using HealthGuard.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class StatisticService
    {
        private readonly HealthContext _context;

        public StatisticService(HealthContext context)
        {
            _context = context;
        }

        public async Task<DashboardStatsDto> GetDashboardStatisticsAsync()
        {
            var topDiseases = await _context.DiagnosisResults
                .GroupBy(r => r.Disease.DiseaseName)
                .Select(g => new TopDiseaseDto
                {
                    DiseaseName = g.Key,
                    CasesCount = g.Count()
                })
                .OrderByDescending(d => d.CasesCount)
                .Take(5)
                .ToListAsync();

            return new DashboardStatsDto
            {
                TotalUsers = await _context.Users.CountAsync(),
                TotalDiagnosticSessions = await _context.DiagnosticSessions.CountAsync(),
                Top5Diseases = topDiseases
            };
        }
    }
}