using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Models.Entity
{
    public class Symptom
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string SymptomCode { get; set; } // VD: SYM01

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } // VD: Ho khan

        public string Description { get; set; }
    }
}