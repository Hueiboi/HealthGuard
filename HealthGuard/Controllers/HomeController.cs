using HealthGuard.Data; // Thêm thư viện này để dùng HealthContext
using HealthGuard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng Include
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    public class HomeController : Controller
    {
        private readonly PatientProfileService _patientProfileService;
        private readonly HealthContext _context; 

        public HomeController(PatientProfileService patientProfileService, HealthContext context)
        {
            _patientProfileService = patientProfileService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                ViewBag.IsGuest = true;
                return View();
            }

            ViewBag.IsGuest = false;
            string username = User.Identity.Name;

            try
            {
                var profile = await _patientProfileService.GetMyProfileAsync(username);

                ViewBag.UserName = !string.IsNullOrEmpty(profile.FullName) && profile.FullName != "Chưa cập nhật tên"
                                    ? profile.FullName
                                    : username;

                if (profile.Height > 0 && profile.Weight > 0)
                {
                    ViewBag.HasHealthRecords = true;

                    double heightInMeters = profile.Height / 100.0;
                    double bmi = Math.Round(profile.Weight / Math.Pow(heightInMeters, 2), 1);

                    ViewBag.BMI = bmi;

                    if (bmi < 18.5) ViewBag.BMIStatus = "Gầy";
                    else if (bmi < 25) ViewBag.BMIStatus = "Bình thường";
                    else if (bmi < 30) ViewBag.BMIStatus = "Thừa cân";
                    else ViewBag.BMIStatus = "Béo phì";
                }
                else
                {
                    ViewBag.HasHealthRecords = false;
                }

                var latestSession = await _context.DiagnosticSessions
                    .Include(s => s.DiagnosisResults)
                        .ThenInclude(dr => dr.Disease)
                    .Where(s => s.User.Username == username)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync(); 

                if (latestSession != null && latestSession.DiagnosisResults.Any())
                {
                    ViewBag.HasRecentDiagnosis = true;
                    ViewBag.DiagnosisDate = latestSession.CreatedAt.ToString("dd MMM, yyyy");

                    var topResult = latestSession.DiagnosisResults.OrderByDescending(r => r.ProbabilityPercentage).First();
                    ViewBag.TopDiseaseName = topResult.Disease.DiseaseName;
                    ViewBag.DiagnosisScore = $"{topResult.ProbabilityPercentage}%";
                }
                else
                {
                    ViewBag.HasRecentDiagnosis = false;
                }
            }
            catch
            {
                ViewBag.UserName = username;
                ViewBag.HasHealthRecords = false;
                ViewBag.HasRecentDiagnosis = false;
            }

            return View();
        }


        
    }
}