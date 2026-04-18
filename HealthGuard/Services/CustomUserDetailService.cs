using HealthGuard.Models.Entities;
using HealthGuard.Models.Entity;
using HealthGuard.Repositories;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    // Service phục vụ cho việc kiểm tra thông tin User trước khi cấp JWT
    public class CustomUserDetailService
    {
        private readonly IUserRepository _userRepository;

        public CustomUserDetailService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // Thay vì trả về UserDetails của Spring, ta trả về entity User
        // Hàm này sẽ gánh vác nhiệm vụ tìm kiếm và kiểm tra trạng thái IsActive
        public async Task<User> LoadUserByUsernameAsync(string emailOrUsername)
        {
            var user = await _userRepository.FindByEmailOrUsernameAsync(emailOrUsername, emailOrUsername);

            if (user == null)
            {
                // Tương đương UsernameNotFoundException
                throw new Exception($"Không tìm thấy user: {emailOrUsername}");
            }

            if (!user.IsActive)
            {
                throw new Exception("Tài khoản đã bị khóa hoặc xóa!");
            }

            // Trả về user hợp lệ để các service khác (như AuthService) dùng tiếp để tạo Token
            return user;
        }
    }
}