using System;
using System.Security.Cryptography;
using System.Text;
using HealthGuard.Models.Dto;
using HealthGuard.Models.Entity;
using Microsoft.CodeAnalysis.Scripting;

// using HealthGuard.Security; // (Nếu bạn có file JwtUtils thì mở ra)
using System;
using System.Threading.Tasks;

namespace HealthGuard.Services
{
    public class AuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly CustomUserDetailService _customUserDetailService;
        private readonly IJwtUtils _jwtUtils;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IPatientRepository patientRepository,
            CustomUserDetailService customUserDetailService,
            IJwtUtils jwtUtils)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _patientRepository = patientRepository;
            _customUserDetailService = customUserDetailService;
            _jwtUtils = jwtUtils;
        }

        public async Task<UserResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            var userRole = await _roleRepository.FindByRoleNameAsync("ROLE_USER");
            if (userRole == null) throw new Exception("Lỗi hệ thống: Không tìm thấy quyền ROLE_USER");

            var newUser = new User
            {
                Username = request.Username,
                // Email = request.Email, // Mở ra nếu DTO/Entity của bạn đã cập nhật thêm Email
                Role = userRole,
                PasswordHash = HashPassword(request.Password),
                IsActive = true
            };

            var savedUser = await _userRepository.SaveAsync(newUser);

            var newPatient = new Patient
            {
                User = savedUser,
                // FullName = request.FullName // Cần chắc chắn RegisterRequestDto của bạn có trường FullName
            };
            await _patientRepository.SaveAsync(newPatient);

            return new UserResponseDto
            {
                Id = savedUser.Id,
                Username = savedUser.Username,
                RoleName = savedUser.Role.RoleName
            };
        }

        public async Task<string> LoginAsync(LoginRequestDto request)
        {
            var user = await _customUserDetailService.LoadUserByUsernameAsync(request.Username);

            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                throw new Exception("Sai tài khoản hoặc mật khẩu!");
            }

            return _jwtUtils.GenerateJwtToken(user);
        }

        // Simple PBKDF2 password hashing to avoid external BCrypt dependency.
        private static string HashPassword(string password)
        {
            const int iterations = 100_000;
            using var rng = RandomNumberGenerator.Create();
            byte[] salt = new byte[16];
            rng.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);
            return $"{iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPassword(string password, string storedHash)
        {
            try
            {
                var parts = storedHash.Split('.');
                if (parts.Length != 3) return false;
                int iterations = int.Parse(parts[0]);
                byte[] salt = Convert.FromBase64String(parts[1]);
                byte[] hash = Convert.FromBase64String(parts[2]);
                using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
                byte[] computed = pbkdf2.GetBytes(hash.Length);
                return CryptographicOperations.FixedTimeEquals(computed, hash);
            }
            catch
            {
                return false;
            }
        }
    }
}