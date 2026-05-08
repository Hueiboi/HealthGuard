
namespace HealthGuard.Models.Entity;

public class DiagnosticSession
{
    public long Id { get; set; }
    public string Status { get; set; } // Đảm bảo có dòng này
    public DateTime CreatedAt { get; set; }
    public virtual User User { get; set; }

    public string MainSymptomDescription { get; set; }
    public int PainLevel { get; set; }
    public virtual ICollection<SessionSymptom> SessionSymptoms { get; set; }
    public virtual ICollection<DiagnosisResult> DiagnosisResults { get; set; }
}