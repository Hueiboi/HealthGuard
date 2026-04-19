using HealthGuard.Data;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity; // Dùng Entity số ít theo cấu trúc của ông
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class RoleService
    {
        private readonly HealthContext _context;

        public RoleService(HealthContext context)
        {
            _context = context;
        }

        public async Task<RoleDto> CreateRoleAsync(RoleDto request)
        {
            string roleName = request.RoleName.ToUpper();
            if (!roleName.StartsWith("ROLE_")) roleName = "ROLE_" + roleName;

            if (await _context.Roles.AnyAsync(r => r.RoleName == roleName))
                throw new InvalidOperationException("Quyền này đã tồn tại!");

            var role = new Role { RoleName = roleName };
            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            return new RoleDto { Id = role.Id, RoleName = role.RoleName };
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            return await _context.Roles
                .Select(r => new RoleDto { Id = r.Id, RoleName = r.RoleName })
                .ToListAsync();
        }

        // ==========================================
        // ĐÃ THÊM LẠI: Hàm lấy chi tiết Quyền (bị thiếu)
        // ==========================================
        public async Task<RoleDto> GetRoleByIdAsync(long id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
                throw new KeyNotFoundException($"Không tìm thấy quyền với ID: {id}");

            return new RoleDto { Id = role.Id, RoleName = role.RoleName };
        }

        // ==========================================
        // ĐÃ THÊM LẠI: Hàm cập nhật Quyền (bị thiếu)
        // ==========================================
        public async Task<RoleDto> UpdateRoleAsync(long id, RoleDto request)
        {
            var existingRole = await _context.Roles.FindAsync(id);

            if (existingRole == null)
                throw new KeyNotFoundException($"Không tìm thấy quyền với ID: {id}");

            // Chuẩn hóa tên quyền mới
            string newRoleName = request.RoleName.ToUpper();
            if (!newRoleName.StartsWith("ROLE_")) newRoleName = "ROLE_" + newRoleName;

            // Kiểm tra xem tên mới có bị trùng với quyền nào khác không
            if (existingRole.RoleName != newRoleName && await _context.Roles.AnyAsync(r => r.RoleName == newRoleName))
                throw new InvalidOperationException("Tên quyền mới đã tồn tại trong hệ thống!");

            // Cập nhật và lưu xuống DB
            existingRole.RoleName = newRoleName;
            await _context.SaveChangesAsync(); // EF Core tự track và sinh lệnh UPDATE

            return new RoleDto { Id = existingRole.Id, RoleName = existingRole.RoleName };
        }

        public async Task DeleteRoleAsync(long id)
        {
            var role = await _context.Roles.FindAsync(id)
                       ?? throw new KeyNotFoundException("Không tìm thấy quyền.");

            if (role.RoleName == "ROLE_ADMIN" || role.RoleName == "ROLE_USER")
                throw new InvalidOperationException("Không được xóa quyền mặc định!");

            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
        }
    }
}