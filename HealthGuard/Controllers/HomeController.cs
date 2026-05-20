using HealthGuard.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboardService;

        public HomeController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                ViewBag.IsGuest = true;
                return View();
            }

            ViewBag.IsGuest = false;

            string username = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Name);

            var data = await _dashboardService.GetDashboardDataAsync(username);

            ViewBag.UserName = data.UserName;
            ViewBag.HasHealthRecords = data.HasHealthRecords;
            ViewBag.BMI = data.BMI;
            ViewBag.BMIStatus = data.BMIStatus;
            ViewBag.Age = data.Age;
            ViewBag.BloodType = data.BloodType;
            ViewBag.HasAllergies = data.HasAllergies;
            ViewBag.AiCount = data.AiCount;

            ViewBag.HasRecentDiagnosis = data.HasRecentDiagnosis;
            ViewBag.DiagnosisDate = data.DiagnosisDate;
            ViewBag.TopDiseaseName = data.TopDiseaseName;
            ViewBag.DiagnosisScore = data.DiagnosisScore;

            return View();
        }
    }
}