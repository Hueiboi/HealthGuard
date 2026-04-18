using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Models.Entity
{
    public class Disease
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string DiseaseCode { get; set; } // VD: DIS01

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // VD: Viêm phế quản

        public string Description { get; set; }
        public string TreatmentAdvice { get; set; } // Lời khuyên điều trị
    }
}