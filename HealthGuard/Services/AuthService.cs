using BCrypt.Net;
using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using HealthGuard.Security;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class AuthService
    {
        private readonly HealthContext _context;
        private readonly IJwtUtils _jwtUtils;

        public AuthService(HealthContext context, IJwtUtils jwtUtils)
        {
            _context = context;
            _jwtUtils = jwtUtils;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            // 1. Chỉ check trùng Email vì form đăng ký không bắt nhập Username nữa
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email đã được sử dụng!");

            // 2. Tự động sinh Username từ Email (VD: nam@gmail.com -> nam)
            string generatedUsername = request.Email.Split('@')[0];

            // Check nhẹ xem username này có ai xài chưa, nếu có thì gắn thêm chuỗi ngẫu nhiên
            if (await _context.Users.AnyAsync(u => u.Username == generatedUsername))
            {
                generatedUsername = $"{generatedUsername}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ROLE_USER")
                            ?? throw new InvalidOperationException("Lỗi hệ thống: Không tìm thấy quyền ROLE_USER");

            // 3. Tạo User mới
            var newUser = new User
            {
                Username = generatedUsername, // Dùng username tự sinh
                Email = request.Email,
                Role = userRole,
                IsActive = true,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(newUser);

            // 4. Tạo Patient mới
            var newPatient = new Patient
            {
                User = newUser,
                FullName = request.FullName, // Lấy tên thật từ Form đăng ký
                MedicalHistory = null
                // ĐÃ XÓA `Gender = null` vì DB của ông không còn cột này nữa
            };

            _context.Patients.Add(newPatient);

            // 5. Lưu 1 lần duy nhất cho cả 2 bảng
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Email = newUser.Email,
                RoleName = newUser.Role.RoleName,
                IsActive = newUser.IsActive,
                CreatedAt = newUser.CreatedAt,
            };
        }

        public async Task<string> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                // Tìm kiếm bằng cả Username HOẶC Email
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Sai tài khoản, mật khẩu hoặc tài khoản bị khóa!");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu!");

            return _jwtUtils.GenerateJwtToken(user);
        }
    }
}