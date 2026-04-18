namespace HealthGuard.Models.Dto
{
    public class SymptomInputDto
    {
        public int SymptomId { get; set; }
        public string SymptomName { get; set; }
        public bool IsSelected { get; set; }
        public int DurationDays { get; set; }
        public string Severity { get; set; } // Mặc định: "Nhẹ"
    }
}