using System.Collections.Generic;

namespace HealthGuard.Models.Dto
{
    // 1. TRANG CHỦ
    public class MobileDashboardDto
    {
        public string Bmi { get; set; }
        public string BmiStatus { get; set; }
        public string TopDiseaseName { get; set; }
        public string DiagnosisScore { get; set; }
    }

    // 2. CHẨN ĐOÁN
    public class MobileDiagnosticRequest
    {
        public string MainSymptomDescription { get; set; }
        public int PainLevel { get; set; }
        public List<long> SymptomIds { get; set; } = new List<long>();
    }

    public class PythonAiResponse
    {
        public string Status { get; set; }
        public List<PythonDiagnosis> Diagnoses { get; set; }
    }

    public class PythonDiagnosis
    {
        public string DiseaseName { get; set; }
        public double Probability { get; set; }
        public string Description { get; set; }
        public string Treatment { get; set; }
    }

    // 3. XÁC THỰC
    public class MobileRegisterRequestDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string? Email { get; set; }
    }

    public class MobileSendOtpRequest
    {
        public string PhoneNumber { get; set; }
    }

    public class MobileVerifyOtpRequest
    {
        public string PhoneNumber { get; set; }
        public string OtpCode { get; set; }
    }
}