using HealthGuard.Data; 
using HealthGuard.Models.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class CustomUserDetailService
    {
        private readonly HealthContext _context;

        public CustomUserDetailService(HealthContext context)
        {
            _context = context;
        }

        public async Task<User> LoadUserByUsernameAsync(string emailOrUsername)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == emailOrUsername || u.Email == emailOrUsername);

            if (user == null)
            {
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