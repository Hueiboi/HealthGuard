using HealthGuard.Data;
using HealthGuard.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class DashboardService
    {
        private readonly HealthContext _context;
        private readonly PatientProfileService _patientProfileService;

        public DashboardService(HealthContext context, PatientProfileService patientProfileService)
        {
            _context = context;
            _patientProfileService = patientProfileService;
        }

        public async Task<DashboardSummaryDto> GetDashboardDataAsync(string username)
        {
            var summary = new DashboardSummaryDto();

            string cleanUsername = username?.Trim().ToLower() ?? "";

            try
            {
                var profile = await _patientProfileService.GetMyProfileAsync(username);
                summary.UserName = !string.IsNullOrEmpty(profile.FullName) && profile.FullName != "Chưa cập nhật tên"
                                    ? profile.FullName
                                    : username;

                if (profile.Height > 0 && profile.Weight > 0)
                {
                    summary.HasHealthRecords = true;
                    double heightInMeters = profile.Height / 100.0;
                    summary.BMI = Math.Round(profile.Weight / Math.Pow(heightInMeters, 2), 1);

                    if (summary.BMI < 18.5) summary.BMIStatus = "Gầy";
                    else if (summary.BMI < 25) summary.BMIStatus = "Bình thường";
                    else if (summary.BMI < 30) summary.BMIStatus = "Thừa cân";
                    else summary.BMIStatus = "Béo phì";
                }
                else
                {
                    summary.HasHealthRecords = false;
                }

                int age = 0;
                if (!string.IsNullOrEmpty(profile.DateOfBirth) && DateTime.TryParse(profile.DateOfBirth, out DateTime dob))
                {
                    age = DateTime.Now.Year - dob.Year;
                    if (DateTime.Now.DayOfYear < dob.DayOfYear) age--;
                }
                summary.Age = age > 0 ? age.ToString() : "--";

                summary.BloodType = string.IsNullOrEmpty(profile.BloodType) ? "Chưa rõ" : profile.BloodType;
                summary.HasAllergies = !string.IsNullOrEmpty(profile.Allergies);


                var allSessions = await _context.DiagnosticSessions
                    .Include(s => s.DiagnosisResults)
                        .ThenInclude(dr => dr.Disease)
                    .Where(s => s.User.Username.ToLower() == cleanUsername || s.User.Email.ToLower() == cleanUsername)
                    .OrderByDescending(s => s.CreatedAt)
                    .ToListAsync();

                summary.AiCount = allSessions.Count;

                var latestSession = allSessions.FirstOrDefault();
                if (latestSession != null && latestSession.DiagnosisResults.Any())
                {
                    summary.HasRecentDiagnosis = true;
                    summary.DiagnosisDate = latestSession.CreatedAt.ToString("dd MMM, yyyy");

                    var topResult = latestSession.DiagnosisResults.OrderByDescending(r => r.ProbabilityPercentage).First();
                    summary.TopDiseaseName = topResult.Disease?.DiseaseName ?? "Không rõ";
                    summary.DiagnosisScore = $"{topResult.ProbabilityPercentage}%";
                }
                else
                {
                    summary.HasRecentDiagnosis = false;
                }
            }
            catch
            {
                summary.UserName = username;
                summary.HasHealthRecords = false;
                summary.HasRecentDiagnosis = false;
                summary.AiCount = 0;
            }

            return summary;
        }
    }
}