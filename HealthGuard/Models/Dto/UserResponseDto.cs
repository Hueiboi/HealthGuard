namespace HealthGuard.Models.Dto
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
