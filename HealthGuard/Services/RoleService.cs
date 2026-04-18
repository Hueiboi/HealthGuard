using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class RoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IRoleMapper _roleMapper;

        public RoleService(IRoleRepository roleRepository, IRoleMapper roleMapper)
        {
            _roleRepository = roleRepository;
            _roleMapper = roleMapper;
        }

        public async Task<RoleDto> CreateRoleAsync(RoleDto request)
        {
            string roleName = request.RoleName?.ToUpper() ?? "";
            if (!roleName.StartsWith("ROLE_")) roleName = "ROLE_" + roleName;

            if (await _roleRepository.FindByRoleNameAsync(roleName) != null)
                throw new Exception("Quyền này đã tồn tại!");

            var role = _roleMapper.ToEntity(request);
            role.RoleName = roleName;

            var savedRole = await _roleRepository.SaveAsync(role);
            return _roleMapper.ToDto(savedRole);
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.FindAllAsync();
            var roleDtos = new List<RoleDto>();
            foreach (var role in roles) roleDtos.Add(_roleMapper.ToDto(role));
            return roleDtos;
        }

        public async Task<RoleDto> GetRoleByIdAsync(int id) // Đổi long -> int
        {
            var role = await _roleRepository.FindByIdAsync(id);
            if (role == null) throw new Exception("Không tìm thấy quyền");
            return _roleMapper.ToDto(role);
        }

        public async Task<RoleDto> UpdateRoleAsync(int id, RoleDto request) // Đổi long -> int
        {
            var existingRole = await _roleRepository.FindByIdAsync(id);
            if (existingRole == null) throw new Exception("Không tìm thấy quyền");

            string newRoleName = request.RoleName?.ToUpper() ?? "";
            if (!newRoleName.StartsWith("ROLE_")) newRoleName = "ROLE_" + newRoleName;

            if (existingRole.RoleName != newRoleName && await _roleRepository.FindByRoleNameAsync(newRoleName) != null)
                throw new Exception("Tên quyền mới đã tồn tại!");

            _roleMapper.UpdateEntityFromDto(request, existingRole);
            existingRole.RoleName = newRoleName;

            var updatedRole = await _roleRepository.SaveAsync(existingRole);
            return _roleMapper.ToDto(updatedRole);
        }

        public async Task DeleteRoleAsync(int id) // Đổi long -> int
        {
            var role = await _roleRepository.FindByIdAsync(id);
            if (role == null) throw new Exception("Không tìm thấy quyền");
            if (role.RoleName == "ROLE_ADMIN" || role.RoleName == "ROLE_USER")
                throw new Exception("Không được phép xóa quyền mặc định!");
            await _roleRepository.DeleteAsync(role);
        }
    }
}