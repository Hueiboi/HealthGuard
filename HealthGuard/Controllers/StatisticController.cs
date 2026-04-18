using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HealthGuard.Services;

namespace HealthGuard.Controllers
{
    [ApiController]
    [Route("api/admin/statistics")]
    // [Authorize(Roles = "ROLE_ADMIN")] 
    public class StatisticController : ControllerBase
    {
        private readonly StatisticService _statisticService;

        public StatisticController(StatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStatsAsync()
        {
            var stats = await _statisticService.GetDashboardStatisticsAsync();
            return Ok(stats);
        }
    }
}