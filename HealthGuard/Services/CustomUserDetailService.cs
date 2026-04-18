using HealthGuard.Models.Entity;
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class CustomUserDetailService
    {
        private readonly IUserRepository _userRepository;

        public CustomUserDetailService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> LoadUserByUsernameAsync(string emailOrUsername)
        {
            var user = await _userRepository.FindByEmailOrUsernameAsync(emailOrUsername, emailOrUsername);

            if (user == null)
            {
                throw new Exception($"Không tìm thấy user: {emailOrUsername}");
            }

            if (!user.IsActive)
            {
                throw new Exception("Tài khoản đã bị khóa hoặc xóa!");
            }

            return user;
        }
    }
}