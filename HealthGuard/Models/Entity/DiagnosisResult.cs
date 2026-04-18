using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class DiagnosisResult
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DiagnosticSession")]
        public int SessionId { get; set; }
        public virtual DiagnosticSession DiagnosticSession { get; set; }

        [ForeignKey("Disease")]
        public int DiseaseId { get; set; }
        public virtual Disease Disease { get; set; }

        public double ProbabilityPercentage { get; set; } // VD: 85.5%
    }
}