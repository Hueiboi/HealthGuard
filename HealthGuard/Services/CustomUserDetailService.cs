using HealthGuard.Data; // Thay bằng namespace chứa HealthContext của ông
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    // Service phục vụ cho việc kiểm tra thông tin User trước khi cấp JWT
    public class CustomUserDetailService
    {
        private readonly HealthContext _context;

        // ĐÃ FIX: Tiêm trực tiếp HealthContext thay cho IUserRepository
        public CustomUserDetailService(HealthContext context)
        {
            _context = context;
        }

        public async Task<User> LoadUserByUsernameAsync(string emailOrUsername)
        {
            // ĐÃ FIX: Dùng LINQ thay cho hàm của Repository
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == emailOrUsername || u.Email == emailOrUsername);

            if (user == null)
            {
                // Dùng UnauthorizedAccessException chuẩn của .NET cho lỗi đăng nhập
                throw new UnauthorizedAccessException($"Không tìm thấy user: {emailOrUsername}");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Tài khoản đã bị khóa hoặc xóa!");
            }

            return user;
        }
    }
}