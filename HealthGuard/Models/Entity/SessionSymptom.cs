
namespace HealthGuard.Models.Entity
{
    public class SessionSymptom
    {
        public long Id { get; set; }
        public int DurationDays { get; set; }
        public string SeverityLevel { get; set; } // Đảm bảo có dòng này
        public long DiagnosticSessionId { get; set; }
        public virtual DiagnosticSession DiagnosticSession { get; set; }
        public long SymptomId { get; set; }
        public virtual Symptom Symptom { get; set; }
    }
}