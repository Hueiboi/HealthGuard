using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class SessionSymptom
    {
        // Dùng Composite Key trong HealthContext
        [ForeignKey("DiagnosticSession")]
        public int SessionId { get; set; }
        public virtual DiagnosticSession DiagnosticSession { get; set; }

        [ForeignKey("Symptom")]
        public int SymptomId { get; set; }
        public virtual Symptom Symptom { get; set; }

        public int DurationDays { get; set; } // Số ngày mắc
        public string Severity { get; set; } // Nhẹ, Vừa, Nặng
    }
}