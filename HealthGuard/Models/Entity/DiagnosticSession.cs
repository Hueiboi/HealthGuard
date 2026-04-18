using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity
{
    public class DiagnosticSession
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
        public virtual User User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}