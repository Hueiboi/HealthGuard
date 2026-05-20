namespace HealthGuard.Models.Dto
{
    public class DashboardSummaryDto
    {
        public string UserName { get; set; }
        public bool HasHealthRecords { get; set; }
        public double BMI { get; set; }
        public string BMIStatus { get; set; }
        public string Age { get; set; }
        public string BloodType { get; set; }
        public bool HasAllergies { get; set; }
        public int AiCount { get; set; }

        public bool HasRecentDiagnosis { get; set; }
        public string DiagnosisDate { get; set; }
        public string TopDiseaseName { get; set; }
        public string DiagnosisScore { get; set; }
    }
}