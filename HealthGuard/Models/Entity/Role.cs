using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Models.Entity
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string RoleName { get; set; } // "ROLE_ADMIN", "ROLE_USER"
    }
}