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
    public class AdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IUserMapper _userMapper;

        public AdminUserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IUserMapper userMapper)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _userMapper = userMapper;
        }

        // Tùy thuộc vào cách bạn thiết kế Repository, hàm này có thể trả về một đối tượng PagedResult tùy chỉnh.
        public async Task<IEnumerable<UserResponseDTO>> GetAllUsersAsync(int page, int size, string keyword)
        {
            // Logic phân trang (Skip/Take) và tìm kiếm thường được viết bên trong Repository
            var users = await _userRepository.FindAllWithPaginationAndSearchAsync(page, size, keyword);

            var userDtos = new List<UserResponseDTO>();
            foreach (var user in users)
            {
                userDtos.Add(_userMapper.ToDTO(user));
            }
            return userDtos;
        }

        public async Task<UserResponseDTO> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.FindByIdAsync(id);
            if (user == null)
            {
                throw new Exception($"Không tìm thấy người dùng có ID: {id}");
            }
            return _userMapper.ToDTO(user);
        }

        public async Task<UserResponseDTO> UpdateUserStatusAsync(long userId, bool isActive)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"Không tìm thấy người dùng với ID: {userId}");
            }

            user.IsActive = isActive;

            var updatedUser = await _userRepository.SaveAsync(user);
            return _userMapper.ToDTO(updatedUser);
        }

        public async Task<UserResponseDTO> ChangeUserRoleAsync(long userId, long roleId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null)
            {
                throw new Exception($"Không tìm thấy người dùng với ID: {userId}");
            }

            var newRole = await _roleRepository.FindByIdAsync(roleId);
            if (newRole == null)
            {
                throw new Exception($"Không tìm thấy quyền với ID: {roleId}");
            }

            user.Role = newRole;

            var updatedUser = await _userRepository.SaveAsync(user);
            return _userMapper.ToDTO(updatedUser);
        }
    }
}