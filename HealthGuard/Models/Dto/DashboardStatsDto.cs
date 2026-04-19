using System.Collections.Generic;

namespace HealthGuard.Models.Dto
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalDiagnosticSessions { get; set; }
        public List<TopDiseaseDto> Top5Diseases { get; set; } = new List<TopDiseaseDto>();
    }
}