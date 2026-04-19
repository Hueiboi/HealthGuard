namespace HealthGuard.Models.Dto
{
    public class PatientProfileDto
    {
        public long Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        // 👉 THÊM MẤY TRƯỜNG NÀY VÀO DTO:
        public string DateOfBirth { get; set; }
        public string EmergencyContact { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string MedicalHistory { get; set; }
    }
}