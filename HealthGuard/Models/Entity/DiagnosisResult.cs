using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class DiagnosisResult
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("DiagnosticSession")]
        public long SessionId { get; set; }
        public virtual DiagnosticSession DiagnosticSession { get; set; }

        [ForeignKey("Disease")]
        public long DiseaseId { get; set; }
        public virtual Disease Disease { get; set; }

        public double ProbabilityPercentage { get; set; } // VD: 85.5%
    }
}