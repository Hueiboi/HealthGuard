using BCrypt.Net;
using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using HealthGuard.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class AuthService
    {
        private readonly HealthContext _context;
        private readonly IJwtUtils _jwtUtils;
        private readonly IMemoryCache _cache;

        public AuthService(HealthContext context, IJwtUtils jwtUtils, IMemoryCache cache)
        {
            _context = context;
            _jwtUtils = jwtUtils;
            _cache = cache;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                throw new InvalidOperationException("Email đã được sử dụng!");

            string generatedUsername = request.Email.Split('@')[0];

            if (await _context.Users.AnyAsync(u => u.Username == generatedUsername))
            {
                generatedUsername = $"{generatedUsername}_{Guid.NewGuid().ToString("N").Substring(0, 4)}";
            }

            var userRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "ROLE_USER")
                            ?? throw new InvalidOperationException("Lỗi hệ thống: Không tìm thấy quyền ROLE_USER");

            var newUser = new User
            {
                Username = generatedUsername, 
                Email = request.Email,
                Role = userRole,
                IsActive = true,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
            };

            _context.Users.Add(newUser);

            var newPatient = new Patient
            {
                User = newUser,
                FullName = request.FullName, 
                MedicalHistory = null
            };

            _context.Patients.Add(newPatient);

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
                .FirstOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Username);

            if (user == null || !user.IsActive)
                throw new UnauthorizedAccessException("Sai tài khoản, mật khẩu hoặc tài khoản bị khóa!");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
                throw new UnauthorizedAccessException("Sai tài khoản hoặc mật khẩu!");

            return _jwtUtils.GenerateJwtToken(user);
        }
       
    }
}