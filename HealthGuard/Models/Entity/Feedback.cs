using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("DiagnosticSession")]
        public int SessionId { get; set; }
        public virtual DiagnosticSession DiagnosticSession { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        [Required]
        public string Comments { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}