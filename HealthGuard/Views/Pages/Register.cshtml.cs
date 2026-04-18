using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HealthGuard.Models.Dto; // Khớp với thư mục Dto bạn đang có
using HealthGuard.Services;
using System.ComponentModel.DataAnnotations;

namespace HealthGuard.Views.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly AuthService _authService;

        // Dependency Injection: Lấy AuthService đã đăng ký trong Program.cs
        public RegisterModel(AuthService authService)
        {
            _authService = authService;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Họ tên không được để trống")]
            public string FullName { get; set; }

            [Required(ErrorMessage = "Email hoặc Username không được để trống")]
            public string UsernameOrEmail { get; set; }

            [Required(ErrorMessage = "Mật khẩu không được để trống")]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            [DataType(DataType.Password)]
            public string ConfirmPassword { get; set; }
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            // Kiểm tra tính hợp lệ của Form (ví dụ: mật khẩu khớp chưa)
            if (!ModelState.IsValid) return Page();

            try
            {
                // Chuyển đổi từ InputModel sang RegisterRequestDto để gửi xuống Service
                var request = new RegisterRequestDto
                {
                    FullName = Input.FullName,
                    Username = Input.UsernameOrEmail,
                    Email = Input.UsernameOrEmail,
                    Password = Input.Password
                };

                await _authService.RegisterAsync(request);

                // Đăng ký xong thì chuyển hướng về trang Login
                return RedirectToPage("Login");
            }
            catch (Exception ex)
            {
                // Nếu có lỗi (ví dụ trùng Username), hiển thị lên giao diện
                ModelState.AddModelError(string.Empty, ex.Message);
                return Page();
            }
        }
    }
}