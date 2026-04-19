using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class DiseaseSymptom
    {
        // Lưu ý: Class này sẽ dùng Composite Key cấu hình trong HealthContext
        [ForeignKey("Disease")]
        public long DiseaseId { get; set; }
        public virtual Disease Disease { get; set; }

        [ForeignKey("Symptom")]
        public long SymptomId { get; set; }
        public virtual Symptom Symptom { get; set; }

        [Required]
        public double WeightScore { get; set; } // Trọng số ảnh hưởng (0.0 đến 1.0)
    }
}