using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class Feedback
    {
        [Key]
        public long Id { get; set; }

        [ForeignKey("DiagnosticSession")]
        public long SessionId { get; set; }
        public virtual DiagnosticSession DiagnosticSession { get; set; }

        [ForeignKey("User")]
        public long UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public string Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}