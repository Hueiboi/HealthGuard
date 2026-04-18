using HealthGuard.Mappers;
using HealthGuard.Models.Dto;
using HealthGuard.Models.DTOs;
using HealthGuard.Models.Entities;
using HealthGuard.Repositories;
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

        public async Task<RoleDTO> CreateRoleAsync(RoleDTO request)
        {
            // 1. Chuẩn hóa tên quyền trước
            string roleName = request.RoleName.ToUpper();
            if (!roleName.StartsWith("ROLE_"))
            {
                roleName = "ROLE_" + roleName;
            }

            // Optional() của Java tương đương check null ở C#
            if (await _roleRepository.FindByRoleNameAsync(roleName) != null)
            {
                throw new Exception("Quyền này đã tồn tại trong hệ thống!");
            }

            // 2. Cập nhật lại DTO với tên đã chuẩn hóa
            request.RoleName = roleName;

            // 3. Map sang Entity
            var role = _roleMapper.ToEntity(request);
            var savedRole = await _roleRepository.SaveAsync(role);

            return _roleMapper.ToDTO(savedRole);
        }

        public async Task<IEnumerable<RoleDTO>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.FindAllAsync();
            var roleDTOs = new List<RoleDTO>();

            foreach (var role in roles)
            {
                roleDTOs.Add(_roleMapper.ToDTO(role));
            }
            return roleDTOs;
        }

        public async Task<RoleDTO> GetRoleByIdAsync(long id)
        {
            var role = await _roleRepository.FindByIdAsync(id);
            if (role == null)
            {
                throw new Exception($"Không tìm thấy quyền với Id: {id}");
            }
            return _roleMapper.ToDTO(role);
        }

        public async Task<RoleDTO> UpdateRoleAsync(long id, RoleDTO request)
        {
            var existingRole = await _roleRepository.FindByIdAsync(id);
            if (existingRole == null)
            {
                throw new Exception($"Không tìm thấy quyền với Id: {id}");
            }

            // 1. Chuẩn hóa tên quyền mới
            string newRoleName = request.RoleName.ToUpper();
            if (!newRoleName.StartsWith("ROLE_"))
            {
                newRoleName = "ROLE_" + newRoleName;
            }

            // So sánh chuỗi trong C# có thể dùng != thay vì !equals()
            if (existingRole.RoleName != newRoleName && await _roleRepository.FindByRoleNameAsync(newRoleName) != null)
            {
                throw new Exception("Tên quyền mới đã tồn tại!");
            }

            // 2. Cập nhật lại tên chuẩn vào DTO trước khi map
            request.RoleName = newRoleName;

            // 3. Tận dụng mapper để đắp dữ liệu mới sang entity cũ
            _roleMapper.UpdateEntityFromDTO(request, existingRole);
            var updatedRole = await _roleRepository.SaveAsync(existingRole);

            return _roleMapper.ToDTO(updatedRole);
        }

        public async Task DeleteRoleAsync(long id)
        {
            var role = await _roleRepository.FindByIdAsync(id);
            if (role == null)
            {
                throw new Exception($"Không tìm thấy quyền với ID: {id}");
            }

            if (role.RoleName == "ROLE_ADMIN" || role.RoleName == "ROLE_USER")
            {
                throw new Exception("Không được phép xóa các quyền mặc định của hệ thống!");
            }

            await _roleRepository.DeleteAsync(role);
        }
    }
}