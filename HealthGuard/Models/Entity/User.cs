using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HealthGuard.Models.Entity 
{
    public class User
    {
        [Key]
        public long Id { get; set; } 

        [Required]
        [MaxLength(50)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress] 
        [MaxLength(100)]
        public string? Email { get; set; } 
        [Required]
        public string? Password { get; set; } 

        [MaxLength(15)]
        public string? PhoneNumber { get; set; } 

        public bool IsActive { get; set; } 

        public DateTime CreatedAt { get; set; } 


        [ForeignKey("Role")]
        public long RoleId { get; set; } 
        public virtual Role Role { get; set; }

        public virtual Patient Patient { get; set; }
    }
}