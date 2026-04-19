using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Models.Dto
{
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
        // Compare là một cú lừa cực mượt của C#: Tự động so sánh trường này với trường Password ở trên
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không trùng khớp!")]
        public string ConfirmPassword { get; set; }
    }
}