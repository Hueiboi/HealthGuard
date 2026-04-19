using HealthGuard.Data; // Thêm thư viện này để dùng HealthContext
using HealthGuard.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Thêm thư viện này để dùng Include
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Controllers
{
    // ĐÃ XÓA [Authorize] ĐỂ KHÁCH VÃNG LAI CÓ THỂ VÀO ĐƯỢC
    public class HomeController : Controller
    {
        private readonly PatientProfileService _patientProfileService;
        private readonly HealthContext _context; // Khai báo thêm Context để gọi DB

        // Bơm cả Service và Context vào
        public HomeController(PatientProfileService patientProfileService, HealthContext context)
        {
            _patientProfileService = patientProfileService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. KIỂM TRA NẾU LÀ KHÁCH VÃNG LAI (CHƯA ĐĂNG NHẬP)
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                ViewBag.IsGuest = true;
                return View(); // Trả về giao diện ngay lập tức
            }

            // 2. NẾU ĐÃ ĐĂNG NHẬP THÌ CHẠY LOGIC XỬ LÝ DỮ LIỆU
            ViewBag.IsGuest = false;
            string username = User.Identity.Name;

            try
            {
                // --- PHẦN 2.1: XỬ LÝ HỒ SƠ & BMI ---
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

                // --- PHẦN 2.2: LẤY DỮ LIỆU CHẨN ĐOÁN AI THẬT TỪ DATABASE ---
                var latestSession = await _context.DiagnosticSessions
                    .Include(s => s.DiagnosisResults)
                        .ThenInclude(dr => dr.Disease)
                    .Where(s => s.User.Username == username)
                    .OrderByDescending(s => s.CreatedAt)
                    .FirstOrDefaultAsync(); // Lấy phiên khám mới nhất

                if (latestSession != null && latestSession.DiagnosisResults.Any())
                {
                    ViewBag.HasRecentDiagnosis = true;
                    ViewBag.DiagnosisDate = latestSession.CreatedAt.ToString("dd MMM, yyyy");

                    // Lấy kết quả bệnh có tỉ lệ % cao nhất trong phiên khám đó
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