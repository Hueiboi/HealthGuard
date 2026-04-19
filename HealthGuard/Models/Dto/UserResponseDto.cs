using System;

namespace HealthGuard.Models.Dto
{
    public class UserResponseDto
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string RoleName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}