using HealthGuard.Models.Dto;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthGuard.Data; // Thay bằng namespace chứa HealthContext của ông

namespace HealthGuard.Services
{
    public class AdminUserService
    {
        private readonly HealthContext _context;

        public AdminUserService(HealthContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int size, string keyword)
        {
            // 1. Tạo Query cơ bản, nhớ Include Role để lấy được RoleName
            var query = _context.Users.Include(u => u.Role).AsQueryable();

            // 2. Xử lý tìm kiếm (Nếu admin có gõ keyword)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                string lowerKeyword = keyword.ToLower();
                // Giả định tìm theo Username hoặc Email
                query = query.Where(u => u.Username.ToLower().Contains(lowerKeyword) ||
                                         u.Email.ToLower().Contains(lowerKeyword));
            }

            // 3. Phân trang, sắp xếp và Map thẳng sang DTO trên câu lệnh SQL
            var users = await query
                .OrderByDescending(u => u.CreatedAt) // Khuyên dùng: Mới tạo thì lên đầu
                .Skip((page - 1) * size)
                .Take(size)
                .Select(u => new UserResponseDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    RoleName = u.Role.RoleName,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(long id)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng có ID: {id}");

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.RoleName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> UpdateUserStatusAsync(long userId, bool isActive)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            // Cập nhật trạng thái
            user.IsActive = isActive;

            // EF Core tự Tracking sự thay đổi, chỉ cần Save
            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.RoleName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        public async Task<UserResponseDto> ChangeUserRoleAsync(long userId, long roleId)
        {
            var user = await _context.Users
                .Include(u => u.Role) // Vẫn phải Include Role cũ để tí nữa còn trả về
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new KeyNotFoundException($"Không tìm thấy người dùng với ID: {userId}");

            // Tìm Role mới
            var newRole = await _context.Roles.FindAsync(roleId);
            if (newRole == null)
                throw new KeyNotFoundException($"Không tìm thấy quyền với ID: {roleId}");

            // Gán Role mới cho User
            user.Role = newRole;

            await _context.SaveChangesAsync();

            return new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                RoleName = user.Role.RoleName, // EF Core sẽ tự lấy tên của Role mới vừa gán
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}