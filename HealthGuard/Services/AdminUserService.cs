using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class AdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserMapper _userMapper;
        private readonly IRoleRepository _roleRepository;

        public AdminUserService(IUserRepository userRepository, IUserMapper userMapper, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _userMapper = userMapper;
            _roleRepository = roleRepository;
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.FindByIdAsync(id);
            if (user == null) throw new Exception($"Không tìm thấy ID: {id}");
            return _userMapper.ToDto(user);
        }

        public async Task<UserResponseDto> UpdateUserStatusAsync(int userId, bool isActive)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            if (user == null) throw new Exception("Không tìm thấy người dùng");

            user.IsActive = isActive;

            var updatedUser = await _userRepository.SaveAsync(user);
            return _userMapper.ToDto(updatedUser);
        }

        public async Task<System.Collections.Generic.IEnumerable<UserResponseDto>> GetAllUsersAsync(int page, int size, string keyword)
        {
            var users = await _userRepository.FindAllWithPaginationAndSearchAsync(page, size, keyword);
            var result = new System.Collections.Generic.List<UserResponseDto>();
            foreach (var u in users)
            {
                result.Add(_userMapper.ToDto(u));
            }
            return result;
        }

        public async Task<UserResponseDto> ChangeUserRoleAsync(int userId, int roleId)
        {
            var user = await _userRepository.FindByIdAsync(userId);
            var newRole = await _roleRepository.FindByIdAsync(roleId);

            if (user == null || newRole == null) throw new Exception("Thông tin không hợp lệ");

            user.Role = newRole;
            user.RoleId = roleId;

            var updatedUser = await _userRepository.SaveAsync(user);
            return _userMapper.ToDto(updatedUser);
        }
    }
}