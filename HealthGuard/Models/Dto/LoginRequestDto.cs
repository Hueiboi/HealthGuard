using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Models.Dto
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập Username")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}